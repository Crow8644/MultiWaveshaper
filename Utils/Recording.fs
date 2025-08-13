// Functions in this module handle live listening
// And recording to a file

module Recording

open NAudio.Wave
open Microsoft.Win32


let startRecording(oversampling: int) =
    match Streams.currentWaveIn with
    | Some(wi) -> 
        wi
        // TODO:
        // Run all waveIn data to a buffer
        // Start a timer with a certain maximum time
        // Stop the recording if either the time runs out or if the user gives input to stop
        true
    | None -> false

// Saves the current final output stream to a user-selected file
let saveAudioFile (fileReader: option<ISampleProvider>) =
    // Test if we are currently working with a file. This will create serious issues if we are not
    if Playback.pause() then                // Make sure pausing was a success
        0L |> Streams.getRepositionFunction // Reset the file to the beginning
        fileReader |> Option.bind Files.saveToUserSelectedStream |> ignore
        true
    else false

let saveCurrentFile () = 
    if Streams.currentFileReader.IsSome then
        Streams.currentFinalOutput |> saveAudioFile
    else
        false