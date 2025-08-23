(* This module holds a mutable list of effects and the operations that might change it
   This is a very OO aproach but easier than the functional approach for an event driven application like this

   Created by: Caleb Ausema (2025)
*)

module EffectOperations

open Effects
open Functionality

let mutable effects: EffectUnion list = List.empty<Effects.EffectUnion>

// Updates all effects functions using current effects
// To be called when something is changed, for example when a new SampleProvider is created
let flush_effects() =
    let x = List.map Effects.getEffectFunction effects

    let ef = Streams.currentEffectProvider
    match ef with
    | Some(provider) -> provider.setEffects x
    | None -> ()

// Create a new effect at the end of the list
let createEffect(effectType: Effects.EffectType) =
    let newEffect = Effects.makeDefault(effectType)

    // Mirrors the same effect for both the effects list and the effect function list
    effects <- 
        List.append effects [newEffect]

    let ef = Streams.currentEffectProvider
    match ef with
    | Some(provider) -> provider.doListProccess(fun x -> 
        List.append x [Effects.getEffectFunction(newEffect)])
    | None -> ()
    newEffect

// Remove an effect from any position, returns true on success and false on failure
let removeIndex(index: int): bool =
    try
        effects <- List.removeAt index effects

        let ef = Streams.currentEffectProvider
        match ef with
        | Some(provider) -> provider.doListProccess(List.removeAt index)
        | None -> ()
        true
    with
        | :? System.IndexOutOfRangeException -> false

// Removes a specific effect rather than at an index
let removeEffect(effect: EffectUnion) =
    let indexOpt = List.tryFindIndex (fun x -> x = effect) effects
    match indexOpt with
    | Some(index) -> removeIndex(index)
    | None -> false

// Move an effect
let moveEffect(sourcePos: int, destinationPos: int) =
    try
        effects <- Utils.swap sourcePos destinationPos effects

        let ef = Streams.currentEffectProvider
        match ef with
        | Some(provider) -> provider.doListProccess(Utils.swap sourcePos destinationPos)
        | None -> ()
        true
    with
        | :? System.IndexOutOfRangeException -> false

//let getChangeBinding (effect: EffectUnion) (port: int) =
//    match effect with
//    | HardLimit (l) -> (fun x -> ())
//    | Volume (v) -> Volume.change_volume v
//    | Smooth (s) -> (fun x -> ())

//let getBinding (effect: EffectUnion) =
//    match effect with
//    | HardLimit (l) -> &l.upperLimit
//    | Volume (v) -> &v.volume
//    | Smooth (s) -> &s.distortionFactor

// Converts an EffectUnion to a dereferenced object, which helps use in UI objects
let unpackEffect (effect: EffectUnion): obj =
    match effect with
    | HardLimit (l) -> l
    | Volume (v) -> v
    | Smooth (s) -> s