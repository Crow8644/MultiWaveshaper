(* Handles the generation and management of 
*)

module Streams

open NAudio
open NAudio.Wave
open EffectSampleProvider
open System.IO
open System
open Functionality

let generateFileProvider (filename: string): option<AudioFileReader> =
    if File.Exists filename then
        Some(new AudioFileReader(filename))
    else
        None

// Creates and sets up a new WaveIn for listening to the device's microphone
let generateLiveListen (): option<WaveInEvent> =
    try
        let w = new WaveInEvent()
        w.RecordingStopped.Add(fun x -> w.Dispose())

        w.StartRecording()
        Some(w)
    with 
        | ex -> None

// The second step in an audio pipeline, generating the EffectProvider and upsampling if necessary
let generateEffectProvider (oversampling: int) (source: ISampleProvider): option<EffectSampleProvider> =
    if oversampling = 1 then
        Some(new EffectSampleProvider(source))
    elif oversampling > 1 then
        let newRate = source.WaveFormat.SampleRate * oversampling

        // The purpose of this chain is run the effects on a higher sample rate
        let upsampler: MediaFoundationResampler = new MediaFoundationResampler(source.ToWaveProvider(), newRate)
        Some (new EffectSampleProvider(upsampler.ToSampleProvider()))
    else
        None

// Create an EffectSampleProvider with the full list of effects from an older one, to keep continuity
let regenerateEffectProvider (lastProvider: EffectSampleProvider) (oversampling: int) (source: ISampleProvider): option<EffectSampleProvider> =
    match generateEffectProvider oversampling source with
    | Some(newProvider) -> 
        newProvider.setEffects lastProvider.getEffects
        Some(newProvider)
    | None -> None

// This function tries to regenerate the provider, and then generates it from scratch if it can't
let tryRegenerateEffectProvider (maybeLastProvider: option<EffectSampleProvider>) (oversampling: int) (source: ISampleProvider): option<EffectSampleProvider> =
    match maybeLastProvider with
    | Some(provider) -> regenerateEffectProvider provider oversampling source
    | None -> generateEffectProvider oversampling source

// Prvoides the last step in our audio pipeline, which may be either downsampling or nothing
let generateFinalOutput (oversampling: int) (source: ISampleProvider): option<ISampleProvider> =
    if oversampling = 1 then
        Some(source)
    elif oversampling > 1 then
        let originalRate: int = source.WaveFormat.SampleRate / oversampling

        let downsampler: MediaFoundationResampler = new MediaFoundationResampler(source.ToWaveProvider(), originalRate)
        Some(downsampler.ToSampleProvider())
    else
        None

//let addMaxHandler (source: WaveInEvent) (action: Action<float32>) =
//    let passMax (sender: obj) (args: WaveInEventArgs) =
        

// Since these important streams
// (1) Cannot be reconstructed easily from current state
// (2) Are accessed asynchronously upon input events from several other modules
// (3) Are expensive to recreate unecessarily when only some of then need to change
// As such, the easiest way to handle them is to break with functional standards and make them mutable values

let mutable currentWaveIn: option<WaveInEvent> = None                       // Saved to avoid recreation when oversampling changes
let mutable currentFileReader: option<AudioFileReader> = None               
let mutable currentEffectProvider: option<EffectSampleProvider> = None      // Accessed by Effects module to set list values
let mutable currentFinalOutput: option<ISampleProvider> = None              // Accessed in order to record to files

let disposeCurrentProviders() =
    match currentWaveIn with
    | Some(w) -> w.StopRecording()
    | None -> ()

    match currentFileReader with
    | Some(fr) -> fr.Dispose()
    | None -> ()

let newFileProvider (oversampling: int) =
    disposeCurrentProviders()
    currentWaveIn <- None

    // This section is somewhat more verbose because we store each value on the way
    // But conceptually, this is just a monadic bind of each generative step
    currentFileReader <- Files.getAudioFile() |> Option.bind generateFileProvider
    currentEffectProvider <- currentFileReader |> Option.bind (tryRegenerateEffectProvider currentEffectProvider oversampling)
    currentFinalOutput <- currentEffectProvider |> Option.bind (generateFinalOutput oversampling)

    currentFinalOutput      // Return and save

let newLiveListen (oversampling: int) =
    disposeCurrentProviders()
    currentFileReader <- None

    currentWaveIn <- generateLiveListen()
    currentEffectProvider <-
        currentWaveIn 
        |> Option.bind (fun x -> Some((new WaveInProvider(x)).ToSampleProvider()))      // Here we have to convert the waveIn into a usable ISampleProvider
        |> Option.bind (tryRegenerateEffectProvider currentEffectProvider oversampling)
    currentFinalOutput <- currentEffectProvider |> Option.bind (generateFinalOutput oversampling)

    currentFinalOutput      // Return and save

let oversamplingChange (oversampling: int) =
    match currentWaveIn with
    | Some(wi) ->
        currentEffectProvider <-
            Some((new WaveInProvider(wi)).ToSampleProvider())
            |> Option.bind (tryRegenerateEffectProvider currentEffectProvider oversampling)
        currentFinalOutput <- currentEffectProvider |> Option.bind (generateFinalOutput oversampling)

        currentFinalOutput
    | None ->
        match currentFileReader with
        | Some(fr) ->
            currentEffectProvider <- currentFileReader |> Option.bind (tryRegenerateEffectProvider currentEffectProvider oversampling)
            currentFinalOutput <- currentEffectProvider |> Option.bind (generateFinalOutput oversampling)

            currentFinalOutput      // Return and save
        | None -> None

// We're unpacking options a lot in these functions because they are for interfacing with the UI, writen in C#
// and I've decided not to expose any option<>s to the C# code

let getRepositionFunction() =
    match currentFileReader with
    | Some(ws) -> (fun x -> 
        // This calculation finds the exact byte position that needs to be set, ensuring that the result be an integer multiple of BlockAlign
        ws.Position <- 
        ws.Length / (int64)ws.BlockAlign |> float 
        |> (*) x |> int64 
        |> (*) (int64 ws.BlockAlign))
    | None -> (fun _ -> ())                     // Return a useless function when doing live processing

let getFileProgress(resulution: int): float =
    //if Monitor.TryEnter switchLock            // This function is called by the GUI thread, so we want no blocking here, just return 0 if unable to attain lock
    match currentFileReader with
    | Some(ws) ->
        let portion: float = float ws.Position / float ws.Length            // Calculates current progress as a portion
        (portion * float resulution)                                        // Multiplies to fit specified range
    | None ->
        0.0

let getFileName(): string =
    match currentFileReader with
    | None ->
        "File Unselected"
    | Some(ws) -> 
        // Returns the name of the file without the extention
        (Files.seperateParentPath ws.FileName)[2]

let getCurrentTime(): Option<System.TimeSpan> =
    match currentFileReader with
    | None ->
        None
    | Some(ws) ->
        Some ws.CurrentTime

let getTotalTime(): Option<System.TimeSpan> =
    match currentFileReader with
    | None ->
        None
    | Some(ws) ->
        Some ws.TotalTime

let getTimeDisplay(): string =
    match getCurrentTime() with
    | Some(t) -> Utils.standardTimeDisplay t
    | None -> "-:--"

let getEndTimeDisplay(): string =
    match getTotalTime() with
    | Some(t) -> Utils.standardTimeDisplay t
    | None -> "-:--"

let isFileOver(): bool = 
    match currentFileReader with
    | None -> false
    | Some(ws) -> (ws.Length <= ws.Position)


// Useful article: https://fsharpforfunandprofit.com/posts/computation-expressions-bind//
// The nature of the UI and my experience doesn't make utilizing it easy, but still