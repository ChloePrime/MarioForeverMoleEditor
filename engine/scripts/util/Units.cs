// #define _16X_KINESIOLOGY_

using Godot;

namespace ChloePrime.MarioForever.Util;

public static class Units
{
    private const float PixelUnitScale =
#if _16X_KINESIOLOGY_
        0.5F;
#else
        1F;
#endif
    
    public static class Speed
    {
        public static float CtfMovementToGd(float speed)
        {
            return PixelUnitScale * speed * 50 / 8;
        }
        
        public static float CtfToGd(float speed)
        {
            return PixelUnitScale * speed * 50;
        }
    }
    
    public static class AngularSpeed
    {
        public static float CtfToGd(float speed)
        {
            return Mathf.DegToRad(Speed.CtfToGd(speed));
        }
    }
    
    public static class Time
    {
        public static float Frame60(float frames)
        {
            return frames / 60;
        }
    }
    
    public static class Acceleration
    {
        public static float CtfMovementToGd(float acc)
        {
            return PixelUnitScale * acc * 50 * 50 / 8;
        }
        
        public static float CtfToGd(float speed)
        {
            return PixelUnitScale * speed * 50 * 50;
        }
    }
}