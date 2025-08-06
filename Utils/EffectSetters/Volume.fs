module Volume

open Effects

let change_volume (v: Volume_Effect) (new_value: float32) =
    v.volume <- new_value