module Effects

open NAudio.Wave
open NAudio.Wave.SampleProviders
open Functionality.Utils

type EffectType =
    | Volume = 0
    | HardDistortion = 1
    | SmoothDistortion = 2
    | Compression = 3
    | Custom = 4

let DELTA: float32 = 0.0001f // The per sample rate of change for effect parameters

// Individual effect types
type Volume2(volume) =
    member this.volume: float32 = volume

type Volume_Effect = 
    {
    mutable smoothed_volume: float32
    mutable volume: float32}
    
type Hard_Distortion_Effect2(upper, lower, makeup) =
    member this.upperLimit: float32 = upper
    member this.lowerLimit: float32 = lower
    member this.makeupGain: bool = makeup

type Hard_Distortion_Effect =
    {
    mutable upperLimit: float32
    mutable upperLimitSmoothed: float32
    mutable lowerLimit: float32
    mutable lowerLimitSmoothed: float32
    mutable makeupGain: bool}

type Smooth_Distortion_Effect =
    // A larger factor makes for a steeper curve and more distortion
    {
    mutable distortionFactor: float32
    mutable smoothFactor: float32}

// A discriminated union which serves as our overaching effect type
type EffectUnion =
    | Volume of Volume_Effect
    | HardLimit of Hard_Distortion_Effect
    | Smooth of Smooth_Distortion_Effect

let volume_function (effect: Volume_Effect) (sample: float32): float32 =
    // Adjusts the volume slightly every frame
    effect.smoothed_volume <- smoothChange effect.smoothed_volume effect.volume

    middleVal (sample * effect.smoothed_volume) -1.0f 1.0f

let hard_distortion_function (effect: Hard_Distortion_Effect) (sample: float32): float32 =
    effect.upperLimitSmoothed <- smoothChange effect.upperLimitSmoothed effect.upperLimit
    effect.lowerLimitSmoothed <- smoothChange effect.lowerLimitSmoothed effect.lowerLimit

    let multiplier = 
        if effect.makeupGain then
            // Calculate make-up gain based on the lesser of the two limits so we don't cause clipping
            1.0f / min effect.upperLimitSmoothed (effect.lowerLimitSmoothed * -1.0f)
        else 1.0f

    if sample > effect.upperLimitSmoothed then
        effect.upperLimitSmoothed * multiplier
    elif sample < effect.lowerLimitSmoothed then
        effect.lowerLimitSmoothed * multiplier
    else
        sample * multiplier

let smooth_distortion_function (effect: Smooth_Distortion_Effect) (sample: float32): float32 =
    effect.smoothFactor <- smoothChange effect.smoothFactor effect.distortionFactor

    // Using this as a multiplier ensures the presence of points (1, 1) and (-1, -1), preventing loss
    let scaler = 1.0f / (tanh effect.smoothFactor)

    sample
    |> (*) effect.smoothFactor
    |> tanh
    |> (*) scaler

// A method to access a partially applied version of a given effect function
let getEffectFunction (effect: EffectUnion): float32->float32 =
    match effect with
    | HardLimit (l) -> hard_distortion_function l
    | Volume (v) -> volume_function v
    | Smooth (s) -> smooth_distortion_function s

// This function is mainly used to store the default values for each type of effect in one place
let makeDefault (effectType: EffectType): EffectUnion =
    match effectType with
        | EffectType.Volume -> 
            Volume({volume = 1.0f; smoothed_volume = 1.0f})
        | EffectType.HardDistortion -> 
            HardLimit({upperLimit = 0.8f; upperLimitSmoothed = 0.8f; lowerLimit = 0.8f; lowerLimitSmoothed = 0.8f; makeupGain = false})
        | EffectType.SmoothDistortion ->
            Smooth({distortionFactor = 2.0f; smoothFactor = 2.0f})