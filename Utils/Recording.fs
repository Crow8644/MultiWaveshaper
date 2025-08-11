// Functions in this module handle live listening
// And recording to a file
// We need to be able to save to a buffer first and then let the user chose the name of a file

module Recording

let startRecording(oversampling: int) =
    match Streams.currentWaveIn with
    | Some(wi) -> 
        wi
        true
    | None -> false