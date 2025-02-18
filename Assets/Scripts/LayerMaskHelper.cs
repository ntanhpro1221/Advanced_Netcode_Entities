public enum LayerMaskHelper : uint {
    Default       = 1 << 0
  , TransparentFX = 1 << 1
  , IgnoreRaycast = 1 << 2
  , Water         = 1 << 4
  , UI            = 1 << 5
  , Ground        = 1 << 6
  , RayCast       = 1 << 7
}