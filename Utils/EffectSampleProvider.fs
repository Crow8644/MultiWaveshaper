module EffectSampleProvider

open NAudio.Wave
open System

type EffectSampleProvider(src: ISampleProvider) =
    let source: ISampleProvider = src

    let mutable effects: (float32->float32) list = List.empty<float32->float32>

    interface ISampleProvider with
        member this.Read (buffer: float32 array, offset: int, count: int): int = 
            let samplesRead = source.Read(buffer, offset, count)

            // Runs each sample through each effect
            Array.mapi 
                (fun (i: int) (s: float32) -> buffer[i] <- (effects |> List.fold (fun acc f -> f acc) s)) 
                buffer
                |> ignore

            samplesRead
        member this.WaveFormat: WaveFormat = 
            source.WaveFormat
    