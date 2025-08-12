// Functions in this module handle live listening
// And recording to a file
// We need to be able to save to a buffer first and then let the user chose the name of a file

module Recording

open NAudio.Wave
open Microsoft.Win32


let startRecording(oversampling: int) =
    match Streams.currentWaveIn with
    | Some(wi) -> 
        wi
        true
    | None -> false

let saveAudioFile() =
    // Test if we are currently working with a file. This will create serious issues if we are not
    match Streams.currentFileReader with
    | Some(reader) ->
        if Playback.pause() then                // Make sure pausing was a success
            0L |> Streams.getRepositionFunction // Reset the file to the beginning
            Streams.currentFinalOutput |> Option.bind Files.saveToUserSelectedStream |> ignore
            true
        else false
    | None ->
        false