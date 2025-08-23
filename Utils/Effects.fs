(* This module contains a bunch of static typing information for effects
   This includes the typing information and base functions for the per sample proccessing of each effect
   There are also functions to acccess this information
  
   Created by: Caleb Auseama (2025)
*)

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

// Individual effect types
type Volume_Effect = 
    {
    mutable smoothed_volume: float32
    mutable volume: float32}

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

type Compression_Effect =
    {
    mutable threshold: float32
    mutable ratio: float32
    mutable attackTimeMilliseconds: int
    mutable releaseTimeMilliseconds: int}

// A discriminated union which serves as our overaching effect type
type EffectUnion =
    | Volume of Volume_Effect
    | HardLimit of Hard_Distortion_Effect
    | Smooth of Smooth_Distortion_Effect
    | Compression of Compression_Effect

// --PROCCESSING FUNCTIONS-- //

let volume_function (effect: Volume_Effect) (sample: float32): float32 =
    // Adjusts the volume slightly every frame
    effect.smoothed_volume <- smoothChange effect.smoothed_volume effect.volume

    middleVal (sample * effect.smoothed_volume) -1.0f 1.0f

let hard_distortion_function (effect: Hard_Distortion_Effect) (sample: float32): float32 =
    effect.upperLimitSmoothed <- smoothChange effect.upperLimitSmoothed effect.upperLimit
    effect.lowerLimitSmoothed <- smoothChange effect.lowerLimitSmoothed effect.lowerLimit

    let multiplier = 
        if effect.makeupGain then
            // Calculate make-up gain based on the higher of the two limits so we don't cause clipping
            1.0f / max effect.upperLimitSmoothed (effect.lowerLimitSmoothed * -1.0f)
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

// --GRAPHICS FUNCTIONS-- //
// Same as the processing functions but without smoothing and taking float instead of float32
// It would be cleaner to have both types of effect functions interface to a set of base functions
// But at least for now, this solution was faster

let volume_graphics (effect: Volume_Effect) (sample: float): float =
    float (middleVal (float32 sample * effect.volume) -1.0f 1.0f)

let hard_distortion_graphics (effect: Hard_Distortion_Effect) (sample: float): float =
    let multiplier = 
        if effect.makeupGain then
            // Calculate make-up gain based on the higher of the two limits so we don't cause clipping
            1.0 / max (float effect.upperLimit) (float effect.lowerLimit * -1.0)
        else 1.0

    if sample > float effect.upperLimit then
        float effect.upperLimit * multiplier
    elif sample < float effect.lowerLimit then
        float effect.lowerLimit * multiplier
    else
        sample * multiplier

let smooth_distortion_graphics (effect: Smooth_Distortion_Effect) (sample: float): float =
    // Using this as a multiplier ensures the presence of points (1, 1) and (-1, -1), preventing loss
    let scaler = 1.0 / (tanh (float effect.distortionFactor))

    sample
    |> (*) (float effect.distortionFactor)
    |> tanh
    |> (*) scaler

// --HELPER FUNCTIONS-- //

// A method to access a partially applied version of a given effect function
let getEffectFunction (effect: EffectUnion): float32->float32 =
    match effect with
    | HardLimit (l) -> hard_distortion_function l
    | Volume (v) -> volume_function v
    | Smooth (s) -> smooth_distortion_function s

let getGraphicsFunction (effect: EffectUnion): float->float =
    match effect with
    | HardLimit (l) -> hard_distortion_graphics l
    | Volume (v) -> volume_graphics v
    | Smooth (s) -> smooth_distortion_graphics s

// This function is mainly used to store the default values for each type of effect in one place
let makeDefault (effectType: EffectType): EffectUnion =
    match effectType with
        | EffectType.Volume -> 
            Volume({volume = 0.8f; smoothed_volume = 0.8f})
        | EffectType.HardDistortion -> 
            HardLimit({upperLimit = 0.8f; upperLimitSmoothed = 0.8f; lowerLimit = -0.8f; lowerLimitSmoothed = -0.8f; makeupGain = false})
        | EffectType.SmoothDistortion ->
            Smooth({distortionFactor = 1.0f; smoothFactor = 1.0f})
        // TODO: Add further effect types