namespace Functionality

open System.Text.RegularExpressions
open System.IO


module Utils =
    let DELTA: float32 = 0.0001f // The per sample rate of change for effect parameters

    let regexSeperate (regex: string) (default_ret: string) (toParse: string) =
        let m = Regex(regex).Match toParse       // Regex matches three groups: the parent folder, the file name, and the file extention
        if m.Success
        then [for x in m.Groups -> x.Value]                             // Composes the original string into the groups from the regex
        else [default_ret]                                              // Default parent directory

    // Uses map-reduce to test if a file fits an abritrarily long list of file extentions
    let matchesExtention (extentions: array<string>) (path: string): bool =
        let ext: string = Path.GetExtension(path)
        extentions |> Array.map(fun s -> (s = ext)) |> Array.reduce(||)

    let standardTimeDisplay (span: System.TimeSpan): string =
        if span.Hours = 0 then
             (string span.Minutes) + ":" + (sprintf "%02d" span.Seconds)
        else
             (string span.Hours) + ":" + (sprintf "%02d" span.Minutes) + ":" + (sprintf "%02d" span.Seconds)

    // Function to test if a value is in the middle of a min and max value
    let middleVal (value: float32) (min: float32) (max: float32) =
        if value < min then min
        elif value > max then max
        else value

    // smoothChange returns the next value that should be used for a particular effect parameter
    let smoothChange (actualVal: float32) (setVal: float32) = 
        // This logic lets an actual prarameter adjust more smoothly than the user's input by adjusting it every frame
        // First we test if the smoothed_volume is exactly equal to volume (no change) and exit early to save on the number of checks performed
        if actualVal = setVal then
            actualVal
        // Second we check if it is more than DELTA away from what the value was set to, and adjust by DELTA
        elif actualVal < setVal - DELTA then
            actualVal + DELTA
        elif actualVal > setVal + DELTA then
            actualVal - DELTA
        // Last, if it is within DELTA distance, we just return
        // Implicitely, this: elif not (actualVal = setVal) then
        else
            setVal