using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBGCustomStats.Integration.JsonObject
{

    public class Class1
    {
        public string MatchId { get; set; }
        public string PingQuality { get; set; }
        public DateTime _D { get; set; }
        public string _T { get; set; }
        public string accountId { get; set; }
        public Common common { get; set; }
        public Character character { get; set; }
        public Vehicle vehicle { get; set; }
        public int elapsedTime { get; set; }
        public int numAlivePlayers { get; set; }
        public int attackId { get; set; }
        public int fireWeaponStackCount { get; set; }
        public Attacker attacker { get; set; }
        public string attackType { get; set; }
        public Weapon weapon { get; set; }
        public bool isLedgeGrab { get; set; }
        public string mapName { get; set; }
        public string weatherId { get; set; }
        public Character1[] characters { get; set; }
        public string cameraViewBehaviour { get; set; }
        public int teamSize { get; set; }
        public bool isCustomGame { get; set; }
        public bool isEventMode { get; set; }
        public string blueZoneCustomOptions { get; set; }
        public int seatIndex { get; set; }
        public Fellowpassenger[] fellowPassengers { get; set; }
        public Item item { get; set; }
        public Gamestate gameState { get; set; }
        public float rideDistance { get; set; }
        public float maxSpeed { get; set; }
        public float distance { get; set; }
        public int phase { get; set; }
        public string[] playersInWhiteCircle { get; set; }
        public string objectType { get; set; }
        public string objectTypeStatus { get; set; }
        public Objecttypeadditionalinfo[] objectTypeAdditionalInfo { get; set; }
        public Parentitem parentItem { get; set; }
        public Childitem childItem { get; set; }
        public bool isVaultOnVehicle { get; set; }
        public string damageTypeCategory { get; set; }
        public string damageCauserName { get; set; }
        public float damage { get; set; }
        public Victim victim { get; set; }
        public string damageReason { get; set; }
        public bool isThroughPenetrableWall { get; set; }
        public Objectlocation objectLocation { get; set; }
        public string[] damageCauserAdditionalInfo { get; set; }
        public float healamount { get; set; }
        public string weaponId { get; set; }
        public int fireCount { get; set; }
        public bool isAttackerInVehicle { get; set; }
        public int dBNOId { get; set; }
        public Reviver reviver { get; set; }
        public bool useTraumaBag { get; set; }
        public Itempackage itemPackage { get; set; }
        public Victimgameresult victimGameResult { get; set; }
        public string victimWeapon { get; set; }
        public string[] victimWeaponAdditionalInfo { get; set; }
        public Victimvehicle victimVehicle { get; set; }
        public Dbnomaker dBNOMaker { get; set; }
        public Dbnodamageinfo dBNODamageInfo { get; set; }
        public Finisher finisher { get; set; }
        public Finishdamageinfo finishDamageInfo { get; set; }
        public Killer killer { get; set; }
        public Killerdamageinfo killerDamageInfo { get; set; }
        public Killervehicle killerVehicle { get; set; }
        public object[] assists_AccountId { get; set; }
        public object[] teamKillers_AccountId { get; set; }
        public bool isSuicide { get; set; }
        public int carePackageUniqueId { get; set; }
        public string carePackageName { get; set; }
        public int ownerTeamId { get; set; }
        public string creatorAccountId { get; set; }
        public string carryState { get; set; }
        public Gameresultonfinished gameResultOnFinished { get; set; }
        public Allweaponstat[] allWeaponStats { get; set; }
    }

    public class Common
    {
        public float isGame { get; set; }
    }

    public class Character
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Item
    {
        public string itemId { get; set; }
        public int stackCount { get; set; }
        public string category { get; set; }
        public string subCategory { get; set; }
        public string[] attachedItems { get; set; }
    }

    public class Vehicle
    {
        public string vehicleType { get; set; }
        public string vehicleId { get; set; }
        public int seatIndex { get; set; }
        public float healthPercent { get; set; }
        public float feulPercent { get; set; }
        public int altitudeAbs { get; set; }
        public int altitudeRel { get; set; }
        public float velocity { get; set; }
        public bool isWheelsInAir { get; set; }
        public bool isInWaterVolume { get; set; }
        public bool isEngineOn { get; set; }
        public Location1 location { get; set; }
    }

    public class Location1
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Attacker
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location2 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location2
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Weapon
    {
        public string itemId { get; set; }
        public int stackCount { get; set; }
        public string category { get; set; }
        public string subCategory { get; set; }
        public string[] attachedItems { get; set; }
    }

    public class Gamestate
    {
        public int elapsedTime { get; set; }
        public int numStartTeams { get; set; }
        public int numAliveTeams { get; set; }
        public int numParticipatedTeams { get; set; }
        public int numJoinPlayers { get; set; }
        public int numStartPlayers { get; set; }
        public int numAlivePlayers { get; set; }
        public int numParticipatedPlayers { get; set; }
        public Safetyzoneposition safetyZonePosition { get; set; }
        public float safetyZoneRadius { get; set; }
        public Poisongaswarningposition poisonGasWarningPosition { get; set; }
        public float poisonGasWarningRadius { get; set; }
        public Redzoneposition redZonePosition { get; set; }
        public int redZoneRadius { get; set; }
        public Blackzoneposition blackZonePosition { get; set; }
        public int blackZoneRadius { get; set; }
    }

    public class Safetyzoneposition
    {
        public float x { get; set; }
        public float y { get; set; }
        public int z { get; set; }
    }

    public class Poisongaswarningposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class Redzoneposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class Blackzoneposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class Objectlocation
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Victim
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location3 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Reviver
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public int health { get; set; }
        public Location4 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location4
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Itempackage
    {
        public string itemPackageId { get; set; }
        public Location5 location { get; set; }
        public Item1[] items { get; set; }
    }

    public class Location5
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Item1
    {
        public string itemId { get; set; }
        public int stackCount { get; set; }
        public string category { get; set; }
        public string subCategory { get; set; }
        public object[] attachedItems { get; set; }
    }

    public class Victimgameresult
    {
        public int rank { get; set; }
        public string gameResult { get; set; }
        public int teamId { get; set; }
        public Stats stats { get; set; }
        public string accountId { get; set; }
        public bool isRewardAbuse { get; set; }
    }

    public class Stats
    {
        public int killCount { get; set; }
        public float distanceOnFoot { get; set; }
        public int distanceOnSwim { get; set; }
        public float distanceOnVehicle { get; set; }
        public float distanceOnParachute { get; set; }
        public float distanceOnFreefall { get; set; }
        public Bprewarddetail bpRewardDetail { get; set; }
        public Arcaderewarddetail arcadeRewardDetail { get; set; }
        public object[] statTrakDataPairs { get; set; }
        public object[] headshotStatTrakDataPairs { get; set; }
    }

    public class Bprewarddetail
    {
        public int byPlayTime { get; set; }
        public int byRanking { get; set; }
        public int byKills { get; set; }
        public int byDamageDealt { get; set; }
        public int boostAmount { get; set; }
        public int byModeScore { get; set; }
    }

    public class Arcaderewarddetail
    {
        public int byPlayTime { get; set; }
    }

    public class Victimvehicle
    {
        public string vehicleType { get; set; }
        public string vehicleId { get; set; }
        public int seatIndex { get; set; }
        public int healthPercent { get; set; }
        public float feulPercent { get; set; }
        public int altitudeAbs { get; set; }
        public int altitudeRel { get; set; }
        public float velocity { get; set; }
        public bool isWheelsInAir { get; set; }
        public bool isInWaterVolume { get; set; }
        public bool isEngineOn { get; set; }
        public Location6 location { get; set; }
    }

    public class Location6
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Dbnomaker
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location7 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location7
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Dbnodamageinfo
    {
        public string damageReason { get; set; }
        public string damageTypeCategory { get; set; }
        public string damageCauserName { get; set; }
        public object[] additionalInfo { get; set; }
        public float distance { get; set; }
        public bool isThroughPenetrableWall { get; set; }
    }

    public class Finisher
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location8 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location8
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Finishdamageinfo
    {
        public string damageReason { get; set; }
        public string damageTypeCategory { get; set; }
        public string damageCauserName { get; set; }
        public string[] additionalInfo { get; set; }
        public float distance { get; set; }
        public bool isThroughPenetrableWall { get; set; }
    }

    public class Killer
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location9 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location9
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Killerdamageinfo
    {
        public string damageReason { get; set; }
        public string damageTypeCategory { get; set; }
        public string damageCauserName { get; set; }
        public string[] additionalInfo { get; set; }
        public float distance { get; set; }
        public bool isThroughPenetrableWall { get; set; }
    }

    public class Killervehicle
    {
        public string vehicleType { get; set; }
        public string vehicleId { get; set; }
        public int seatIndex { get; set; }
        public int healthPercent { get; set; }
        public int feulPercent { get; set; }
        public int altitudeAbs { get; set; }
        public int altitudeRel { get; set; }
        public int velocity { get; set; }
        public bool isWheelsInAir { get; set; }
        public bool isInWaterVolume { get; set; }
        public bool isEngineOn { get; set; }
        public Location10 location { get; set; }
    }

    public class Location10
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class Parentitem
    {
        public string itemId { get; set; }
        public int stackCount { get; set; }
        public string category { get; set; }
        public string subCategory { get; set; }
        public string[] attachedItems { get; set; }
    }

    public class Childitem
    {
        public string itemId { get; set; }
        public int stackCount { get; set; }
        public string category { get; set; }
        public string subCategory { get; set; }
        public object[] attachedItems { get; set; }
    }

    public class Gameresultonfinished
    {
        public Result[] results { get; set; }
    }

    public class Result
    {
        public int rank { get; set; }
        public string gameResult { get; set; }
        public int teamId { get; set; }
        public Stats1 stats { get; set; }
        public string accountId { get; set; }
        public bool isRewardAbuse { get; set; }
    }

    public class Stats1
    {
        public int killCount { get; set; }
        public float distanceOnFoot { get; set; }
        public int distanceOnSwim { get; set; }
        public float distanceOnVehicle { get; set; }
        public float distanceOnParachute { get; set; }
        public float distanceOnFreefall { get; set; }
        public Bprewarddetail1 bpRewardDetail { get; set; }
        public Arcaderewarddetail1 arcadeRewardDetail { get; set; }
        public object[] statTrakDataPairs { get; set; }
        public object[] headshotStatTrakDataPairs { get; set; }
    }

    public class Bprewarddetail1
    {
        public int byPlayTime { get; set; }
        public int byRanking { get; set; }
        public int byKills { get; set; }
        public int byDamageDealt { get; set; }
        public int boostAmount { get; set; }
        public int byModeScore { get; set; }
    }

    public class Arcaderewarddetail1
    {
        public int byPlayTime { get; set; }
    }

    public class Character1
    {
        public Character2 character { get; set; }
        public string primaryWeaponFirst { get; set; }
        public string primaryWeaponSecond { get; set; }
        public string secondaryWeapon { get; set; }
        public int spawnKitIndex { get; set; }
    }

    public class Character2
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location11 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public string[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location11
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Fellowpassenger
    {
        public string name { get; set; }
        public int teamId { get; set; }
        public float health { get; set; }
        public Location12 location { get; set; }
        public int ranking { get; set; }
        public int individualRanking { get; set; }
        public string accountId { get; set; }
        public bool isInBlueZone { get; set; }
        public bool isInRedZone { get; set; }
        public string inSpecialZone { get; set; }
        public bool isInVehicle { get; set; }
        public object[] zone { get; set; }
        public string type { get; set; }
    }

    public class Location12
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Objecttypeadditionalinfo
    {
        public string first { get; set; }
        public int second { get; set; }
    }

    public class Allweaponstat
    {
        public string accountId { get; set; }
        public Stat[] stats { get; set; }
    }

    public class Stat
    {
        public string weapon { get; set; }
        public int damage { get; set; }
        public int dBNODamage { get; set; }
        public int shots { get; set; }
        public int hits { get; set; }
        public int dBNOHits { get; set; }
        public int holdingTime { get; set; }
        public Hitdetail[] hitDetails { get; set; }
    }

    public class Hitdetail
    {
        public string bodyPart { get; set; }
        public int kills { get; set; }
        public int dBNOs { get; set; }
        public int hits { get; set; }
        public int dBNOHits { get; set; }
        public int damage { get; set; }
        public int dBNODamage { get; set; }
    }
}