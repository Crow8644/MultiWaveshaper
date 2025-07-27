module Playback
open NAudio.Wave
open Functionality
open System.Threading
open System.Threading.Tasks

// Playback's custom exception for threading errors
exception PlaybackThreadingError of string

let outputDevice: WaveOutEvent = new WaveOutEvent()
let stopSignal: ManualResetEvent = new ManualResetEvent(false)          // This works as a mutex and allows us to synchronize events with the playback stopping

// This handler makes the OutEvent reset itself after being stopped, meaning if the user hits play afterward, it will already be
outputDevice.PlaybackStopped.Add(fun args -> 
    stopSignal.Set() |> ignore
    // Resets the file by invoking a function from module Streams, which will change as the file changes
    Streams.getRepositionFunction 0
)

let initializeFromFileUnchecked(oversampling: int) =
    Files.getAudioFile()
    |> Streams.newFileProvider oversampling
    |> Option.map outputDevice.Init
    |> ignore

let oversamplingChangeUnchecked(oversampling: int) =
    Streams.oversamplingChange(oversampling)
    |> outputDevice.Init
    
// All three of the following functions return true on success and false on failure

// This plays the audio, either from the beginning or after a pause. It does not reset the audio
let play() =
    try
        outputDevice.Play()
        true
    with | ex -> false

// Pauses the audio with the ability to be started back up, plackbackStopped is not invoked
let pause() =
    try
        outputDevice.Pause()
        true
    with | ex -> false

// Ends the audio device playback, meaning playbackStopped will be signalled
let stop() =
    try
        outputDevice.Stop()                 // Causes a very sudden stop to the audio
        // Because PlaybackStopped on a file is set up to reset the sample provider to the beginning, this will reset the file
        true
    with | ex -> false

let stopAndDo (nextAction: _->unit) (errorAction: _->unit)=
    // Tests if the device is already stopped, so the passed action still happens
    if outputDevice.PlaybackState = PlaybackState.Stopped then 
        nextAction()
    // stopSignal may have been set several times before this point, so we need to reset it
    elif stopSignal.Reset() then                    // Reset will return a true if successful and a false if not

        // IMPORTANT: Because the event handlers for PlaybackStopped are ran in the thread stop was called from,
        // The program freezes if we call stop and wait in the same thread
        let parellel() =
            stopSignal.WaitOne() |> ignore          // Wait for the playbackStopped signal before the action
            nextAction()
            
        let t: Task = new Task(parellel)            // Sets the task with internal function
        t.Start()

        outputDevice.Stop()                         // Pause the audio and signal playbackStopped, causing our other thread to run its actions
    else
        errorAction()

let newFile(oversampling: int) = stopAndDo (fun _ -> initializeFromFileUnchecked oversampling) (fun _ -> raise (PlaybackThreadingError("Signal could not be reset")))

let oversamplingChanged(oversampling: int) = stopAndDo (fun _ -> oversamplingChangeUnchecked oversampling) (fun _ -> raise (PlaybackThreadingError("Signal could not be reset")))

let closeObjects() =
    // Adds the continuation to the PlaybackStopped event, so they will only trigger after playback has stopped
    outputDevice.PlaybackStopped.Add(fun args ->
        outputDevice.Dispose()
        Streams.closeObjects()      // Passes on responsability for any sample providers to the Streams module
    )

    outputDevice.Stop()             // Eventually triggers the above, but stopping is on another thread and takes a moment