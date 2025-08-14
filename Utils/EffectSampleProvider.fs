(* This is an object that impliments NAudio's ISampleProvider interface
   It is to be used as part of an audio pipeline and can hold a list of simple float32->float32 functions as effects
   This is a very OO file and the reason it's in F# is that I like the elegance of the higher order functions

   Created by: Caleb Ausema
*)

module EffectSampleProvider

open NAudio.Wave
open System
open System.Threading

type EffectSampleProvider(src: ISampleProvider) =
    let source: ISampleProvider = src

    let listLock: obj = new obj()

    let mutable effects: (float32->float32) list = List.empty<float32->float32>

    member this.setEffects (newList: (float32->float32) list) =
        lock listLock (fun _ ->
            effects <- newList
        )

    member this.getEffects = effects

    // Applies a funtion to effects and resaves it, protecting the mutable list
    member this.doListProccess (proccess: ((float32->float32) list -> (float32->float32) list)) =
        lock listLock (fun _ ->
            effects <- proccess effects
        )

    interface ISampleProvider with
        member this.Read (buffer: float32 array, offset: int, count: int): int = 
            let samplesRead = source.Read(buffer, offset, count)

            // Runs each sample through each effect
            lock listLock (fun _ -> 
            Array.map
                (fun (i: int) -> buffer[i] <- (effects |> List.fold (fun acc f -> f acc) buffer[i]))
                [|offset..(offset + samplesRead - 1)|]              // List of every necessary buffer index
                |> ignore
            )

            samplesRead
        member this.WaveFormat: WaveFormat = 
            source.WaveFormat
    