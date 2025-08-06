module HardClipping

open Effects

let change_makeup_gain (l: Hard_Distortion_Effect) (new_value: bool) =
    l.makeupGain <- new_value

    // !! Test this
    // Do we need to update the change in the effect provider?

let change_lower (l: Hard_Distortion_Effect) (new_value: float32) =
    l.lowerLimit <- new_value

let change_upper (l: Hard_Distortion_Effect) (new_value: float32) =
    l.upperLimit <- new_value
