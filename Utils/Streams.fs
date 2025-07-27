module Streams

open NAudio
open NAudio.Wave
open EffectSampleProvider

let mutable currentEffectProvider: option<EffectSampleProvider> = None
let mutable currentFileReader: option<AudioFileReader> = None                // I imagine this will stay none if we're working with live audio

//let w = new SampleChannel(new WaveInProvider(new WaveIn()))

// Actions:

let makeStandard (source: ISampleProvider) =
    currentEffectProvider <- Some(new EffectSampleProvider(source))
    currentEffectProvider.Value

let makeOversampler (factor: int) (source: ISampleProvider) =
    let originalRate = source.WaveFormat.SampleRate
    let newRate = originalRate * factor

    let upsampler: MediaFoundationResampler = new MediaFoundationResampler(source.ToWaveProvider(), newRate)
    currentEffectProvider <- Some (new EffectSampleProvider(upsampler.ToSampleProvider()))

    let downsampler: MediaFoundationResampler = new MediaFoundationResampler(currentEffectProvider.Value.ToWaveProvider(), originalRate)
    downsampler.ToSampleProvider()

// Get new provider
let oversamplingChange(oversamplingRate: int) =
    if oversamplingRate > 1 then
        if currentFileReader.IsSome then
            makeOversampler oversamplingRate currentFileReader.Value
        else
            // TODO: Handle live playback in general
            currentEffectProvider.Value
    else
        // This section runs if we use a 
        match currentFileReader with
        | Some(ws) ->
            currentEffectProvider <- Some(new EffectSampleProvider(ws))
        | None ->
            let filename = Files.getAudioFile()
            currentFileReader <- Some(new AudioFileReader(filename.Value))
            currentEffectProvider <- Some(new EffectSampleProvider(currentFileReader.Value))

        currentEffectProvider.Value // Return the effect provider, so playback can work well

// TODO: Finish oversampling
let newFileProvider (oversamplingRate: int) (filename: option<string>): option<ISampleProvider> =
    if filename.IsSome then
        currentFileReader <- Some(new AudioFileReader(filename.Value))
        if oversamplingRate > 1 then
            Some(makeOversampler oversamplingRate currentFileReader.Value)
        else
            // Using this function makes consistancy with above
            // And serves to change the type from option<EffectSampleProvider> to option<ISampleProvider> (vs. just returning currentEffectProvider)
            Some(makeStandard currentFileReader.Value)
    else None

// Get current provider and repositioning function

// Returns a function that can be used to set the position in the track
let getRepositionFunction =
    match currentFileReader with
    | Some(ws) -> (fun x -> ws.Position <- x)
    | None -> (fun _ -> ())                     // Return a useless function when doing live processing

let closeObjects() =
    match currentFileReader with
    | Some(ws) -> ws.Dispose()
    | None -> ()
