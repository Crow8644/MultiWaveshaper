module Effects

open NAudio.Wave
open NAudio.Wave.SampleProviders
open Functionality.Utils

type EffectType =
    | Volume = 0
    | Limiter = 1
    | SmoothDistortion = 2
    | Compression = 3
    | Custom = 4

// Individual effect types
type Volume_Effect(volume) =
    member this.volume: float32 = volume
    
type Limiter_Effect(upper, lower, makeup) =
    member this.upperLimit: float32 = upper
    member this.lowerLimit: float32 = lower
    member this.makeupGain: bool = makeup

// A discriminated union which serves as our overaching effect type
type EffectUnion =
    | Volume of Volume_Effect
    | Limiter of Limiter_Effect

let volume_function (effect: Volume_Effect) (sample: float32): float32 =
    middleVal (sample * effect.volume) 1.0f -1.0f

let limiter_function (effect: Limiter_Effect) (sample: float32): float32 =
    let multiplier = 
        if effect.makeupGain then
            // Calculate make-up gain based on the lesser of the two limits so we don't cause clipping
            1.0f / min effect.upperLimit (effect.lowerLimit * -1.0f)
        else 1.0f

    if sample > effect.upperLimit then
        effect.upperLimit * multiplier
    elif sample < effect.lowerLimit then
        effect.lowerLimit * multiplier
    else
        sample * multiplier

// A method to access a partially applied version of a given effect function
let getEffectFunction (effect: EffectUnion): float32->float32 =
    match effect with
    | Limiter (l) -> limiter_function l
    | Volume (v) -> volume_function v

