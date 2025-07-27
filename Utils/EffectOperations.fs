module EffectOperations

open Effects

let mutable effects: EffectUnion list = List.empty<Effects.EffectUnion>

let mutable current_effect: EffectUnion = Limiter(new Effects.Limiter_Effect(1.0f, 1.0f, false))

// Create an effect
let createEffect(effectType: Effects.EffectType) =
    // Built in here are the defaults for each effect type
    match effectType with
    | Effects.EffectType.Limiter -> current_effect <- Limiter(new Effects.Limiter_Effect(0.8f, 0.8f, false))
    | _ -> ()
    
    effects <-
        List.append effects [current_effect]

// Remove an effect
let removeEffect(position: int) =
    effects <- List.removeAt position effects

    // Streams.getCurrentProvider()

// Move an effect
let moveEffect(sourcePos: int, destinationPos: int) =
    ()

// Change an effect parameter