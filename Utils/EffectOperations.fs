module EffectOperations

open Effects

let mutable effects: EffectUnion list = List.empty<Effects.EffectUnion>

let mutable current_effect: EffectUnion = Volume({volume = 1.0f; smoothed_volume = 1.0f})

// Updates all effects functions using current effects
// To be called when something is changed, for example when a new SampleProvider is created
let flush_effects() =
    let x = List.map Effects.getEffectFunction effects

    Streams.getCurrentEffectProvider().Value.setEffects x


// Create an effect
let createEffect(effectType: Effects.EffectType) =
    // Built in here are the defaults for each effect type
    if Streams.getCurrentEffectProvider().IsSome then
        match effectType with
        | Effects.EffectType.HardDistortion -> 
            current_effect <- HardLimit({upperLimit = 0.8f; lowerLimit = 0.8f; makeupGain = false})
        | Effects.EffectType.Volume -> 
            current_effect <- Volume({volume = 1.0f; smoothed_volume = 1.0f})
        | _ -> ()

        // Mirrors the same effect for both the effects list and the effect function list
        effects <- 
            List.append effects [current_effect]
        Streams.getCurrentEffectProvider().Value.doListProccess(fun x -> 
            List.append x [Effects.getEffectFunction(current_effect)])
        true
    else false
    

// Remove an effect
let removeEffect(position: int) =
    effects <- List.removeAt position effects

    // Streams.getCurrentProvider()

// Move an effect
let moveEffect(sourcePos: int, destinationPos: int) =
    ()

let getChangeBinding(port: int) =
    match current_effect with
    | HardLimit (l) -> (fun x -> ())
    | Volume (v) -> Volume.change_volume v
    | Smooth (s) -> (fun x -> ())

let getBinding() =
    match current_effect with
    | HardLimit (l) -> &l.upperLimit
    | Volume (v) -> &v.volume
    | Smooth (s) -> &s.distortionFactor