namespace Code.TimeSystem
{
    public static class TimeUtil
    {
        //이건 현실 시간 기준임
        // ex 현실 시간 기준을 게임 기준 시간으로 변환 해줌
        public static float Min(float v) => v ;
        public static float Hour(float v) => v * 60f;
        public static float Day(float v) => v * 3600f;
    }
}