namespace ChloePrime.MarioForever.Util;

public static class Units
{
    public static class Speed
    {
        public static float CtfMovementToGd(float speed)
        {
            return speed * 50 / 8;
        }
        
        public static float CtfToGd(float speed)
        {
            return speed * 50;
        }
    }
    
    public static class Acceleration
    {
        public static float CtfMovementToGd(float acc)
        {
            return acc * 50 * 50 / 8;
        }
        
        public static float CtfToGd(float speed)
        {
            return speed * 50 * 50;
        }
    }
}