using Ami.BroAudio;
using Code.DataSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;
using SHS.Scripts.Crosshairs;

namespace Scripts.Combat.Datas
{
    public enum GunType
    {
        Pistol, // 권총
        Smg, // 기관단총
        AssaultRifle, // AR
        SniperRifle, // 저격총
        Shotgun, // 산탄총
    }

    [CreateAssetMenu(fileName = "GunDataSO", menuName = "SO/Item/GunData", order = 0)]
    public class GunDataSO : WeaponDataSO
    {
        [Header("GunDataInfo")] [ExcelColumn("gunType")] // 총 타입
        public GunType gunType;

        [ExcelColumn("bulletPerShot")] // 발당 총알 수
        public int bulletPerShot = 1;

        [ExcelColumn("reloadTime")] // 장전 시간
        public float reloadTime;

        [ExcelColumn("bulletSpeed")] // 총알 속도
        public float bulletSpeed;

        [ExcelColumn("maxRange")] // 총알 최대 사거리. 0이면 attackRange를 사용
        public float maxRange;

        [ExcelColumn("maxAmmoCapacity")] // 탄창 용량
        public int maxAmmoCapacity;

        [ExcelColumn("aimSpeedMultiplier")] // 조준시 이동속도 배율
        public float aimSpeedMultiplier;

        [ExcelColumn("noiseRadius")]  // 총소리 반경
        public float noiseRadius = 10f;

        // public float noiseVolume = 0.5f; // 총소리 크기

        [Header("SpreadData")] [ExcelColumn("defaultSpread")] // 기본 탄퍼짐
        public float defaultSpread = 0.324f;

        [SerializeField] public CrosshairSO crosshairData; // 크로스헤어 데이터

        [ExcelColumn("maxSpread")] // 최대 탄퍼짐
        public float maxSpread = 0.945f;

        [ExcelColumn("spreadGrow")] // 발사시 퍼짐 증가
        public float spreadGrow = 0.27f;

        [ExcelColumn("spreadRecover")] // 초당 퍼짐 회복
        public float spreadRecover = 0.4f;

        [ExcelColumn("spreadFactor")] // 퍼짐 계수
        public float spreadFactor = 10.79f;

        [Header("Base recoil (units)")] [ExcelColumn("verticalRecoil")] // 수직 반동
        public float verticalRecoil = 50f;

        [ExcelColumn("horizontalRecoil")] // 수평 반동
        public float horizontalRecoil = 50f;

        [Header("Per-shot random multipliers (min/max)")] [ExcelColumn("minVerticalMultiplier")] // 최소 수직 반동 배율
        public float minVerticalMultiplier = 0.95f;

        [ExcelColumn("maxVerticalMultiplier")] // 최대 수직 반동 배율
        public float maxVerticalMultiplier = 1.05f;

        [ExcelColumn("minHorizontalMultiplier")] // 최소 수평 반동 배율
        public float minHorizontalMultiplier = -0.35f;

        [ExcelColumn("maxHorizontalMultiplier")] // 최대 수평 반동 배율
        public float maxHorizontalMultiplier = 0.65f;

        [Header("RecoilData")] [ExcelColumn("recoilDuration")] // 반동 지속 시간
        public float recoilDuration = 0.075f;

        [ExcelColumn("recoilRecoveryStartTime")] // 반동 회복 시작 시간
        public float recoilRecoveryStartTime = 0.10f;

        [ExcelColumn("recoilRecoveryTime")] // 반동 회복 시간
        public float recoilRecoveryTime = 0.12f;

        [ExcelColumn("recoilRecovery")] // 반동 회복 속도
        public float recoilRecovery = 550f;

        [ExcelColumn("pixelsPerRecoilUnit")] // 반동 픽셀 변환 계수
        public float pixelsPerRecoilUnit = 0.45f;

        public SoundID reloadSound;

        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new GunItem(this), maxSpawnCount);
        }
    }
}
