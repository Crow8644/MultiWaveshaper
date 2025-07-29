module EffectOperations

open Effects

let mutable effects: EffectUnion list = List.empty<Effects.EffectUnion>

let mutable current_effect: EffectUnion = Limiter(new Effects.Limiter_Effect(1.0f, 1.0f, false))

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
        | Effects.EffectType.Limiter -> current_effect <- Limiter(new Effects.Limiter_Effect(0.8f, 0.8f, false))
        | Effects.EffectType.Volume -> current_effect <- Volume(new Effects.Volume_Effect(1.0f))
        | _ -> ()

        // Mirrors the same effect for both the effects list and the effect function list
        effects <- 
            List.append effects [current_effect]
        Streams.getCurrentEffectProvider().Value.doListProccess(fun x -> 
            List.append x [Effects.getEffectFunction(current_effect)])
    

// Remove an effect
let removeEffect(position: int) =
    effects <- List.removeAt position effects

    // Streams.getCurrentProvider()

// Move an effect
let moveEffect(sourcePos: int, destinationPos: int) =
    ()

// Change an effect parameter