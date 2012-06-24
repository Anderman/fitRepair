using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myFit
{
    public struct FIELD_COMPONENT
    {
        float scale;
        float offset;
        byte num;
        byte bits;
        bool accumulate;
        public FIELD_COMPONENT(float scale, float offset, byte num, byte bits, bool accumulate)
        {
            this.scale = scale;
            this.offset = offset;
            this.num = num;
            this.bits = bits;
            this.accumulate = accumulate;
        }
    } ;
    public struct SUBFIELD_MAP
    {
        UInt32 refFieldValue;
        byte refFieldNum;
        public SUBFIELD_MAP(UInt32 refFieldValue, byte refFieldNum)
        {
            this.refFieldValue = refFieldValue;
            this.refFieldNum = refFieldNum;
        }
    } ;
    public struct SUBFIELD
    {
        SUBFIELD_MAP[] maps;
        string name;
        string units;
        float scale;
        float offset;
        byte numMaps;
        byte type;
        public SUBFIELD(SUBFIELD_MAP[] maps, string name, string units, float scale, float offset, byte numMaps, byte type)
        {
            this.maps = maps;
            this.name = name;
            this.units = units;
            this.scale = scale;
            this.offset = offset;
            this.numMaps = numMaps;
            this.type = type;
        }
    } ;
    public struct globalField
    {
        FIELD_COMPONENT[] components;
        SUBFIELD[] subFields;
        public string name;
        public string units;
        public double scale;
        public double offset;
        public int numComponents;
        public int numSubFields;
        public byte num;
        public byte type;
        public int foutMarge;
        public bool valid;

        public globalField(byte num)
        {
            this.components = null;
            this.subFields = null;
            this.name = "Unknown";
            this.units = "";
            this.scale = 1;
            this.offset = 0;
            this.numComponents = 0;
            this.numSubFields = 0;
            this.num = num;
            this.type = 0;
            this.foutMarge = -1;
            this.valid = false;
        }
        public globalField(FIELD_COMPONENT[] components, SUBFIELD[] subFields, string name, string units, float scale, float offset, int numComponents, int numSubFields, byte num, byte type)
        {
            this.components = components;
            this.subFields = subFields;
            this.name = name;
            this.units = units;
            this.scale = scale;
            this.offset = offset;
            this.numComponents = numComponents;
            this.numSubFields = numSubFields;
            this.num = num;
            this.type = type;
            this.foutMarge = -1;
            this.valid = true;
        }
        public globalField(FIELD_COMPONENT[] components, SUBFIELD[] subFields, string name, string units, float scale, float offset, int numComponents, int numSubFields, byte num, byte type, int foutMarge)
        {
            this.components = components;
            this.subFields = subFields;
            this.name = name;
            this.units = units;
            this.scale = scale;
            this.offset = offset;
            this.numComponents = numComponents;
            this.numSubFields = numSubFields;
            this.num = num;
            this.type = type;
            this.foutMarge = foutMarge;
            this.valid = true;
        }
    } ;
    public struct MESG
    {
        public globalField[] fields;
        public string name;
        public int num;
        public int numFields;
        public MESG(globalField[] fields, string name, int num, int numFields)
        {
            this.fields = fields;
            this.name = name;
            this.num = num;
            this.numFields = numFields;
        }
    } ;
    static class FIT
    {
        public static DateTime dt1989 = new DateTime(1989, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        public static CultureInfo cuNL = new CultureInfo("nl-NL");
        public static string timestampToLocalTime(UInt32 UNCTimestamp)
        {
            return dt1989.AddSeconds(UNCTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
        enum EVENT_TYPE
        {
            START = 0,
            STOP = 1,
            CONSECUTIVE_DEPRECIATED = 2,
            MARKER = 3,
            STOP_ALL = 4,
            BEGIN_DEPRECIATED = 5,
            END_DEPRECIATED = 6,
            END_ALL_DEPRECIATED = 7,
            STOP_DISABLE = 8,
            STOP_DISABLE_ALL = 9
        }
        enum TIMER_TRIGGER
        {
            MANUAL = 0,
            AUTO = 1,
            FITNESS_EQUIPMENT = 2,

        }
        enum EVENT
        {
            TIMER = 0, // Group 0.  Start / stop_all
            WORKOUT = 3, // start / stop
            WORKOUT_STEP = 4, // Start at beginning of workout.  Stop at end of each step.
            POWER_DOWN = 5, // stop_all group 0
            POWER_UP = 6, // stop_all group 0
            OFF_COURSE = 7, // start / stop group 0
            SESSION = 8, // Stop at end of each session.
            LAP = 9, // Stop at end of each lap.
            COURSE_POINT = 10, // marker
            BATTERY = 11, // marker
            VIRTUAL_PARTNER_PACE = 12, // Group 1. Start at beginning of activity if VP enabled, when VP pace is changed during activity or VP enabled mid activity.  stop_disable when VP disabled.
            HR_HIGH_ALERT = 13, // Group 0.  Start / stop when in alert condition.
            HR_LOW_ALERT = 14, // Group 0.  Start / stop when in alert condition.
            SPEED_HIGH_ALERT = 15, // Group 0.  Start / stop when in alert condition.
            SPEED_LOW_ALERT = 16, // Group 0.  Start / stop when in alert condition.
            CAD_HIGH_ALERT = 17, // Group 0.  Start / stop when in alert condition.
            CAD_LOW_ALERT = 18, // Group 0.  Start / stop when in alert condition.
            POWER_HIGH_ALERT = 19, // Group 0.  Start / stop when in alert condition.
            POWER_LOW_ALERT = 20, // Group 0.  Start / stop when in alert condition.
            RECOVERY_HR = 21, // marker
            BATTERY_LOW = 22, // marker
            TIME_DURATION_ALERT = 23, // Group 1.  Start if enabled mid activity (not required at start of activity,. Stop when duration is reached.  stop_disable if disabled.
            DISTANCE_DURATION_ALERT = 24, // Group 1.  Start if enabled mid activity (not required at start of activity,. Stop when duration is reached.  stop_disable if disabled.
            CALORIE_DURATION_ALERT = 25, // Group 1.  Start if enabled mid activity (not required at start of activity,. Stop when duration is reached.  stop_disable if disabled.
            ACTIVITY = 26, // Group 1..  Stop at end of activity.
            FITNESS_EQUIPMENT = 27, // marker
            LENGTH = 28, // Stop at end of each length.
        }
        enum DEVICE_TYPE
        {
            ANTFS = 1,
            BIKE_POWER = 11,
            ENVIRONMENT_SENSOR = 12,
            MULTI_SPORT_SPEED_DISTANCE = 15,
            FITNESS_EQUIPMENT = 17,
            BLOOD_PRESSURE = 18,
            WEIGHT_SCALE = 119,
            HEART_RATE = 120,
            BIKE_SPEED_CADENCE = 121,
            BIKE_CADENCE = 122,
            BIKE_SPEED = 123,
            STRIDE_SPEED_DISTANCE = 124
        }
        enum GARMIN_PRODUCT
        {
            HRM1 = 1,
            AXH01 = 2, // AXH01 HRM chipset
            AXB01 = 3,
            AXB02 = 4,
            HRM2SS = 5,
            DSI_ALF02 = 6,
            FR405 = 717, // Forerunner 405
            FR50 = 782, // Forerunner 50
            FR60 = 988, // Forerunner 60
            DSI_ALF01 = 1011,
            FR310XT = 1018, // Forerunner 310
            EDGE500 = 1036,
            FR110 = 1124, // Forerunner 110
            EDGE800 = 1169,
            CHIRP = 1253,
            EDGE200 = 1325,
            FR910XT = 1328,
            ALF04 = 1341,
            FR610 = 1345,
            FR70 = 1436,
            FR310XT_4T = 1446,
            AMX = 1461,
            SDM4 = 10007, // SDM4 footpod
            TRAINING_CENTER = 20119,
            CONNECT = 65534, // Garmin Connect website
        }

        enum SWIM_STROKE
        {
            FREESTYLE = 0,
            BACKSTROKE = 1,
            BREASTSTROKE = 2,
            BUTTERFLY = 3,
            DRILL = 4,
            MIXED = 5
        }
        enum ACTIVITY_TYPE
        {
            GENERIC = 0,
            RUNNING = 1,
            CYCLING = 2,
            TRANSITION = 3, // Mulitsport transition
            FITNESS_EQUIPMENT = 4,
            SWIMMING = 5,
            WALKING = 6,
            ALL = 254, // All is for goals only to include all sports.
        }
        enum ACTIVITY_SUBTYPE
        {
            GENERIC = 0,
            TREADMILL = 1, // Run
            STREET = 2, // Run
            TRAIL = 3, // Run
            TRACK = 4, // Run
            SPIN = 5, // Cycling
            INDOOR_CYCLING = 6, // Cycling
            ROAD = 7, // Cycling
            MOUNTAIN = 8, // Cycling
            DOWNHILL = 9, // Cycling
            RECUMBENT = 10, // Cycling
            CYCLOCROSS = 11, // Cycling
            HAND_CYCLING = 12, // Cycling
            TRACK_CYCLING = 13, // Cycling
            INDOOR_ROWING = 14, // Fitness Equipment
            ELLIPTICAL = 15, // Fitness Equipment
            STAIR_CLIMBING = 16, // Fitness Equipment
            LAP_SWIMMING = 17, // Swimming
            OPEN_WATER = 18, // Swimming
            ALL = 254,
        }
        enum DISPLAY_MEASURE
        {
            METRIC = 0,
            STATUTE = 1
        }
        enum INTENSITY
        {
            ACTIVE = 0,
            REST = 1,
            WARMUP = 2,
            COOLDOWN = 3
        }
        enum LAP_TRIGGER
        {
            MANUAL = 0,
            TIME = 1,
            DISTANCE = 2,
            POSITION_START = 3,
            POSITION_LAP = 4,
            POSITION_WAYPOINT = 5,
            POSITION_MARKED = 6,
            SESSION_END = 7,
            FITNESS_EQUIPMENT = 8
        }
        public static byte StringToEnum(string fieldName, string value)
        {
            switch (fieldName)
            {
                case "device_type": { DEVICE_TYPE val; return Enum.TryParse<DEVICE_TYPE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "event_type": { EVENT_TYPE val; return Enum.TryParse<EVENT_TYPE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "timer_trigger": { TIMER_TRIGGER val; return Enum.TryParse<TIMER_TRIGGER>(value, out val) ? (byte)val : byte.Parse(value); }
                case "type": { DEVICE_TYPE val; return Enum.TryParse<DEVICE_TYPE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "event": { EVENT val; return Enum.TryParse<EVENT>(value, out val) ? (byte)val : byte.Parse(value); }
                case "sport": { ACTIVITY_TYPE val; return Enum.TryParse<ACTIVITY_TYPE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "sub_sport": { ACTIVITY_SUBTYPE val; return Enum.TryParse<ACTIVITY_SUBTYPE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "trigger": { EVENT_TYPE val; return Enum.TryParse<EVENT_TYPE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "swim_stroke": { SWIM_STROKE val; return Enum.TryParse<SWIM_STROKE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "pool_length_unit": { DISPLAY_MEASURE val; return Enum.TryParse<DISPLAY_MEASURE>(value, out val) ? (byte)val : byte.Parse(value); }
                case "intensity": { INTENSITY val; return Enum.TryParse<INTENSITY>(value, out val) ? (byte)val : byte.Parse(value); }
                case "lap_trigger": { LAP_TRIGGER val; return Enum.TryParse<LAP_TRIGGER>(value, out val) ? (byte)val : byte.Parse(value); }
            }
            return byte.Parse(value);
        }
        public static string EnumToString(string fieldName, int value)
        {
            switch (fieldName)
            {
                case "device_type": return Enum.IsDefined(typeof(DEVICE_TYPE), value) ? ((DEVICE_TYPE)value).ToString() : value.ToString();
                case "event_type": return Enum.IsDefined(typeof(EVENT_TYPE), value) ? ((EVENT_TYPE)value).ToString() : value.ToString();
                case "timer_trigger": return Enum.IsDefined(typeof(TIMER_TRIGGER), value) ? ((TIMER_TRIGGER)value).ToString() : value.ToString();
                case "type": return Enum.IsDefined(typeof(DEVICE_TYPE), value) ? ((DEVICE_TYPE)value).ToString() : value.ToString();
                case "event": return Enum.IsDefined(typeof(EVENT), value) ? ((EVENT)value).ToString() : value.ToString();
                case "sport": return Enum.IsDefined(typeof(ACTIVITY_TYPE), value) ? ((ACTIVITY_TYPE)value).ToString() : value.ToString();
                case "sub_sport": return Enum.IsDefined(typeof(ACTIVITY_SUBTYPE), value) ? ((ACTIVITY_SUBTYPE)value).ToString() : value.ToString();
                case "trigger": return Enum.IsDefined(typeof(EVENT_TYPE), value) ? ((EVENT_TYPE)value).ToString() : value.ToString();
                case "swim_stroke": return Enum.IsDefined(typeof(SWIM_STROKE), value) ? ((SWIM_STROKE)value).ToString() : value.ToString();
                case "pool_length_unit": return Enum.IsDefined(typeof(DISPLAY_MEASURE), value) ? ((DISPLAY_MEASURE)value).ToString() : value.ToString();
                case "intensity": return Enum.IsDefined(typeof(INTENSITY), value) ? ((INTENSITY)value).ToString() : value.ToString();
                case "lap_trigger": return Enum.IsDefined(typeof(LAP_TRIGGER), value) ? ((LAP_TRIGGER)value).ToString() : value.ToString();
            }
            return value.ToString();
        }
        public static globalField getFieldStruct(MESG msg, int FieldDefinitionNumber)
        {
            foreach (globalField field in msg.fields)
                if (field.num == FieldDefinitionNumber)
                    return field;
            return new globalField((byte)FieldDefinitionNumber);
        }
        public static globalField getFieldStruct(int GlobalMsgIndex, int FieldDefinitionNumber)
        {
            foreach (globalField field in getMessageStruct(GlobalMsgIndex).fields)
                if (field.num == FieldDefinitionNumber)
                    return field;
            return new globalField((byte)FieldDefinitionNumber);
        }
        public static MESG getMessageStruct(int GlobalMsgIndex)
        {
            foreach (MESG msg in mesgs)
                if (msg.num == GlobalMsgIndex)
                    return msg;
            return new MESG(unknownFields, "Unknown", 0, 0);
        }

        public const int FIT_MESG_NUM_FILE_ID = 0;
        public const int FIT_MESG_NUM_CAPABILITIES = 1;
        public const int FIT_MESG_NUM_DEVICE_SETTINGS = 2;
        public const int FIT_MESG_NUM_USER_PROFILE = 3;
        public const int FIT_MESG_NUM_HRM_PROFILE = 4;
        public const int FIT_MESG_NUM_SDM_PROFILE = 5;
        public const int FIT_MESG_NUM_BIKE_PROFILE = 6;
        public const int FIT_MESG_NUM_ZONES_TARGET = 7;
        public const int FIT_MESG_NUM_HR_ZONE = 8;
        public const int FIT_MESG_NUM_POWER_ZONE = 9;
        public const int FIT_MESG_NUM_MET_ZONE = 10;
        public const int FIT_MESG_NUM_SPORT = 12;
        public const int FIT_MESG_NUM_GOAL = 15;
        public const int FIT_MESG_NUM_SESSION = 18;
        public const int FIT_MESG_NUM_LAP = 19;
        public const int FIT_MESG_NUM_RECORD = 20;
        public const int FIT_MESG_NUM_EVENT = 21;
        public const int FIT_MESG_NUM_SOURCE = 22;
        public const int FIT_MESG_NUM_DEVICE_INFO = 23;
        public const int FIT_MESG_NUM_WORKOUT = 26;
        public const int FIT_MESG_NUM_WORKOUT_STEP = 27;
        public const int FIT_MESG_NUM_SCHEDULE = 28;
        public const int FIT_MESG_NUM_WEIGHT_SCALE = 30;
        public const int FIT_MESG_NUM_COURSE = 31;
        public const int FIT_MESG_NUM_COURSE_POINT = 32;
        public const int FIT_MESG_NUM_TOTALS = 33;
        public const int FIT_MESG_NUM_ACTIVITY = 34;
        public const int FIT_MESG_NUM_SOFTWARE = 35;
        public const int FIT_MESG_NUM_FILE_CAPABILITIES = 37;
        public const int FIT_MESG_NUM_MESG_CAPABILITIES = 38;
        public const int FIT_MESG_NUM_FIELD_CAPABILITIES = 39;
        public const int FIT_MESG_NUM_FILE_CREATOR = 49;
        public const int FIT_MESG_NUM_BLOOD_PRESSURE = 51;
        public const int FIT_MESG_NUM_MONITORING = 55;
        public const int FIT_MESG_NUM_HRV = 78;
        public const int FIT_MESG_NUM_LENGTH = 101;
        public const int FIT_MESG_NUM_MONITORING_INFO = 103;
        public const int FIT_MESG_NUM_PAD = 105;
        public const int FIT_MESG_NUM_MFG_RANGE_MIN = 0xFF00; // 0xFF00 - 0xFFFE reserved for manufacturer specific messages
        public const int FIT_MESG_NUM_MFG_RANGE_MAX = 0xFFFE; // 0xFF00 - 0xFFFE reserved for manufacturer specific messages
        static SUBFIELD_MAP[] sessionAvgCadenceAvgRunningCadenceMaps = { new SUBFIELD_MAP(1, 5) }; // sport == running                
        static SUBFIELD_MAP[] sessionMaxCadenceMaxRunningCadenceMaps = { new SUBFIELD_MAP(1, 5) }; // sport == running            
        static SUBFIELD_MAP[] sessionTotalCyclesTotalStridesMaps = { new SUBFIELD_MAP(1, 5) }; // sport == running
        static SUBFIELD[] sessionAvgCadenceSubFields = { new SUBFIELD(sessionAvgCadenceAvgRunningCadenceMaps, "avg_running_cadence", "strides/min", 1, 0, 1, 2) };
        static SUBFIELD[] sessionMaxCadenceSubFields = { new SUBFIELD(sessionMaxCadenceMaxRunningCadenceMaps, "max_running_cadence", "strides/min", 1, 0, 1, 2) };
        static SUBFIELD[] sessionTotalCyclesSubFields = { new SUBFIELD(sessionTotalCyclesTotalStridesMaps, "total_strides", "strides", 1, 0, 1, 134) };
        static SUBFIELD_MAP[] lapAvgCadenceAvgRunningCadenceMaps = { new SUBFIELD_MAP(1, 5) }; // sport == running                
        static SUBFIELD_MAP[] lapMaxCadenceMaxRunningCadenceMaps = { new SUBFIELD_MAP(1, 5) }; // sport == running            
        static SUBFIELD_MAP[] lapTotalCyclesTotalStridesMaps = { new SUBFIELD_MAP(1, 5) }; // sport == running
        static SUBFIELD[] lapAvgCadenceSubFields = { new SUBFIELD(lapAvgCadenceAvgRunningCadenceMaps, "avg_running_cadence", "strides/min", 1, 0, 1, 2) };
        static SUBFIELD[] lapMaxCadenceSubFields = { new SUBFIELD(lapMaxCadenceMaxRunningCadenceMaps, "max_running_cadence", "strides/min", 1, 0, 1, 2) };
        static SUBFIELD[] lapTotalCyclesSubFields = { new SUBFIELD(lapTotalCyclesTotalStridesMaps, "total_strides", "strides", 1, 0, 1, 134) };
        static FIELD_COMPONENT[] recordCompressedSpeedDistanceComponents =
            {
                new FIELD_COMPONENT(100, 0, 6, 12, false), // speed
                new FIELD_COMPONENT(16, 0, 5, 12, true), // distance
            };
        static FIELD_COMPONENT[] recordCyclesComponents = { new FIELD_COMPONENT(1, 0, 19, 8, true) }; // total_cycles
        static FIELD_COMPONENT[] recordCompressedAccumulatedPowerComponents = { new FIELD_COMPONENT(1, 0, 29, 16, true) }; // accumulated_power
        static FIELD_COMPONENT[] eventData16Components = { new FIELD_COMPONENT((float)1, (float)0, 3, 16, true) }; // data
        static SUBFIELD_MAP[] eventDataTimerTriggerMaps = { new SUBFIELD_MAP(0, 0) }; // event == timer          
        static SUBFIELD_MAP[] eventDataCoursePointIndexMaps = { new SUBFIELD_MAP(10, 0) }; // event == course_point
        static SUBFIELD_MAP[] eventDataBatteryLevelMaps = { new SUBFIELD_MAP(11, 0) }; // event == battery         
        static SUBFIELD_MAP[] eventDataVirtualPartnerSpeedMaps = { new SUBFIELD_MAP(12, 0) }; // event == virtual_partner_pace                
        static SUBFIELD_MAP[] eventDataHrHighAlertMaps = { new SUBFIELD_MAP(13, 0) }; // event == hr_high_alert                
        static SUBFIELD_MAP[] eventDataHrLowAlertMaps = { new SUBFIELD_MAP(14, 0) }; // event == hr_low_alert                
        static SUBFIELD_MAP[] eventDataSpeedHighAlertMaps = { new SUBFIELD_MAP(15, 0) }; // event == speed_high_alert
        static SUBFIELD_MAP[] eventDataSpeedLowAlertMaps = { new SUBFIELD_MAP(16, 0) }; // event == speed_low_alert          
        static SUBFIELD_MAP[] eventDataCadHighAlertMaps = { new SUBFIELD_MAP(17, 0) }; // event == cad_high_alert            
        static SUBFIELD_MAP[] eventDataCadLowAlertMaps = { new SUBFIELD_MAP(18, 0) }; // event == cad_low_alert
        static SUBFIELD_MAP[] eventDataPowerHighAlertMaps = { new SUBFIELD_MAP(19, 0) }; // event == power_high_alert        
        static SUBFIELD_MAP[] eventDataPowerLowAlertMaps = { new SUBFIELD_MAP(20, 0) }; // event == power_low_alert          
        static SUBFIELD_MAP[] eventDataTimeDurationAlertMaps = { new SUBFIELD_MAP(23, 0) }; // event == time_duration_alert  
        static SUBFIELD_MAP[] eventDataDistanceDurationAlertMaps = { new SUBFIELD_MAP(24, 0) }; // event == distance_duration_alert                
        static SUBFIELD_MAP[] eventDataCalorieDurationAlertMaps = { new SUBFIELD_MAP(25, 0) }; // event == calorie_duration_alert                
        static SUBFIELD_MAP[] eventDataFitnessEquipmentStateMaps = { new SUBFIELD_MAP(27, 0) }; // event == fitness_equipment                
        static SUBFIELD[] eventDataSubFields = {
           new SUBFIELD(eventDataTimerTriggerMaps, "timer_trigger", "", 1, 0, 1, 0),
           new SUBFIELD(eventDataCoursePointIndexMaps, "course_point_index", "", 1, 0, 1, 132),
           new SUBFIELD(eventDataBatteryLevelMaps, "battery_level", "V", 1000, 0, 1, 132),
           new SUBFIELD(eventDataVirtualPartnerSpeedMaps, "virtual_partner_speed", "m/s", 1000, 0, 1, 132),
           new SUBFIELD(eventDataHrHighAlertMaps, "hr_high_alert", "bpm", 1, 0, 1, 2),
           new SUBFIELD(eventDataHrLowAlertMaps, "hr_low_alert", "bpm", 1, 0, 1, 2),
           new SUBFIELD(eventDataSpeedHighAlertMaps, "speed_high_alert", "m/s", 1000, 0, 1, 132),
           new SUBFIELD(eventDataSpeedLowAlertMaps, "speed_low_alert", "m/s", 1000, 0, 1, 132),
           new SUBFIELD(eventDataCadHighAlertMaps, "cad_high_alert", "rpm", 1, 0, 1, 132),
           new SUBFIELD(eventDataCadLowAlertMaps, "cad_low_alert", "rpm", 1, 0, 1, 132),
           new SUBFIELD(eventDataPowerHighAlertMaps, "power_high_alert", "watts", 1, 0, 1, 132),
           new SUBFIELD(eventDataPowerLowAlertMaps, "power_low_alert", "watts", 1, 0, 1, 132),
           new SUBFIELD(eventDataTimeDurationAlertMaps, "time_duration_alert", "s", 1000, 0, 1, 134),
           new SUBFIELD(eventDataDistanceDurationAlertMaps, "distance_duration_alert", "m", 100, 0, 1, 134),
           new SUBFIELD(eventDataCalorieDurationAlertMaps, "calorie_duration_alert", "calories", 1, 0, 1, 134),
           new SUBFIELD(eventDataFitnessEquipmentStateMaps, "fitness_equipment_state", "", 1, 0, 1, 0)
        };
        static public globalField[] unknownFields = {
           new globalField( null, null, "message_index", "", 1, 0, 0, 0, 254, 132 ),
           new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400)
        };
        static SUBFIELD_MAP[] fileIdProductGarminProductMaps ={ 
            new SUBFIELD_MAP( 1, 1 ), // manufacturer == garmin
            new SUBFIELD_MAP( 15, 1 ),// manufacturer == dynastream
            new SUBFIELD_MAP( 13, 1 ) // manufacturer == dynastream_oem
        };
        static SUBFIELD[] fileIdProductSubFields = { new SUBFIELD(fileIdProductGarminProductMaps, "garmin_product", "", 1, 0, 3, 132) };

        static public globalField[] fileIdFields = {
           new globalField( null, null, "type", "", 1, 0, 0, 0, 0, 0),
           new globalField( null, null, "manufacturer", "", 1, 0, 0, 0, 1, 132,0),
           new globalField( null, fileIdProductSubFields, "product", "", 1, 0, 0, 1, 2, 132),
           new globalField( null, null, "serial_number", "", 1, 0, 0, 0, 3, 140,0),
           new globalField( null, null, "time_created", "", 1, 0, 0, 0, 4, 134,2678400),
           new globalField( null, null, "number", "", 1, 0, 0, 0, 5, 132),
        };

        static public globalField[] capabilitiesFields;
        static public globalField[] deviceSettingsFields;
        static public globalField[] userProfileFields;
        static public globalField[] hrmProfileFields;
        static public globalField[] sdmProfileFields;
        static public globalField[] bikeProfileFields;
        static public globalField[] zonesTargetFields;
        static public globalField[] hrZoneFields;
        static public globalField[] powerZoneFields;
        static public globalField[] metZoneFields;
        static public globalField[] sportFields;
        static public globalField[] goalFields;
        static public globalField[] sessionFields = {
           new globalField( null, null, "message_index", "", 1, 0, 0, 0, 254, 132 ),
           new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400),
           new globalField( null, null, "event", "", 1, 0, 0, 0, 0, 0),
           new globalField( null, null, "event_type", "", 1, 0, 0, 0, 1, 0),
           new globalField( null, null, "start_time", "", 1, 0, 0, 0, 2, 134),
           new globalField( null, null, "start_position_lat", "semicircles", 1, 0, 0, 0, 3, 133),
           new globalField( null, null, "start_position_long", "semicircles", 1, 0, 0, 0, 4, 133),
           new globalField( null, null, "sport", "", 1, 0, 0, 0, 5, 0),
           new globalField( null, null, "sub_sport", "", 1, 0, 0, 0, 6, 0),
           new globalField( null, null, "total_elapsed_time", "s", 1000, 0, 0, 0, 7, 134),
           new globalField( null, null, "total_timer_time", "s", 1000, 0, 0, 0, 8, 134),
           new globalField( null, null, "total_distance", "m", 100, 0, 0, 0, 9, 134),
           new globalField( null, sessionTotalCyclesSubFields, "total_cycles", "cycles", 1, 0, 0, 1, 10, 134),
           new globalField( null, null, "total_calories", "kcal", 1, 0, 0, 0, 11, 132),
           new globalField( null, null, "total_fat_calories", "kcal", 1, 0, 0, 0, 13, 132),
           new globalField( null, null, "avg_speed", "m/s", 1000, 0, 0, 0, 14, 132),
           new globalField( null, null, "max_speed", "m/s", 1000, 0, 0, 0, 15, 132),
           new globalField( null, null, "avg_heart_rate", "bpm", 1, 0, 0, 0, 16, 2),
           new globalField( null, null, "max_heart_rate", "bpm", 1, 0, 0, 0, 17, 2),
           new globalField( null, sessionAvgCadenceSubFields, "avg_cadence", "rpm", 1, 0, 0, 1, 18, 2),
           new globalField( null, sessionMaxCadenceSubFields, "max_cadence", "rpm", 1, 0, 0, 1, 19, 2),
           new globalField( null, null, "avg_power", "watts", 1, 0, 0, 0, 20, 132),
           new globalField( null, null, "max_power", "watts", 1, 0, 0, 0, 21, 132),
           new globalField( null, null, "total_ascent", "m", 1, 0, 0, 0, 22, 132),
           new globalField( null, null, "total_descent", "m", 1, 0, 0, 0, 23, 132),
           new globalField( null, null, "total_training_effect", "", 10, 0, 0, 0, 24, 2),
           new globalField( null, null, "first_lap_index", "", 1, 0, 0, 0, 25, 132),
           new globalField( null, null, "num_laps", "", 1, 0, 0, 0, 26, 132),
           new globalField( null, null, "event_group", "", 1, 0, 0, 0, 27, 2),
           new globalField( null, null, "trigger", "", 1, 0, 0, 0, 28, 0),
           new globalField( null, null, "nec_lat", "semicircles", 1, 0, 0, 0, 29, 133),
           new globalField( null, null, "nec_long", "semicircles", 1, 0, 0, 0, 30, 133),
           new globalField( null, null, "swc_lat", "semicircles", 1, 0, 0, 0, 31, 133),
           new globalField( null, null, "swc_long", "semicircles", 1, 0, 0, 0, 32, 133),
           new globalField( null, null, "normalized_power", "watts", 1, 0, 0, 0, 34, 132),
           new globalField( null, null, "training_stress_score", "tss", 10, 0, 0, 0, 35, 132),
           new globalField( null, null, "intensity_factor", "if", 1000, 0, 0, 0, 36, 132),
           new globalField( null, null, "left_right_balance", "", 1, 0, 0, 0, 37, 132),
           new globalField( null, null, "avg_stroke_count", "strokes", 10, 0, 0, 0, 41, 134),
           new globalField( null, null, "avg_stroke_distance", "m", 100, 0, 0, 0, 42, 132),
           new globalField( null, null, "swim_stroke", "swim_stroke", 1, 0, 0, 0, 43, 0),
           new globalField( null, null, "pool_length", "m", 100, 0, 0, 0, 44, 132),
           new globalField( null, null, "pool_length_unit", "", 1, 0, 0, 0, 46, 0),
           new globalField( null, null, "num_active_lengths", "lengths", 1, 0, 0, 0, 47, 132),
           new globalField( null, null, "total_work", "J", 1, 0, 0, 0, 48, 134),
           new globalField( null, null, "avg_altitude", "m", 5, 500, 0, 0, 49, 132),
           new globalField( null, null, "max_altitude", "m", 5, 500, 0, 0, 50, 132),
           new globalField( null, null, "gps_accuracy", "m", 1, 0, 0, 0, 51, 2),
           new globalField( null, null, "avg_grade", "%", 100, 0, 0, 0, 52, 131),
           new globalField( null, null, "avg_pos_grade", "%", 100, 0, 0, 0, 53, 131),
           new globalField( null, null, "avg_neg_grade", "%", 100, 0, 0, 0, 54, 131),
           new globalField( null, null, "max_pos_grade", "%", 100, 0, 0, 0, 55, 131),
           new globalField( null, null, "max_neg_grade", "%", 100, 0, 0, 0, 56, 131),
           new globalField( null, null, "avg_temperature", "C", 1, 0, 0, 0, 57, 1),
           new globalField( null, null, "max_temperature", "C", 1, 0, 0, 0, 58, 1),
           new globalField( null, null, "total_moving_time", "s", 1000, 0, 0, 0, 59, 134),
           new globalField( null, null, "avg_pos_vertical_speed", "m/s", 1000, 0, 0, 0, 60, 131),
           new globalField( null, null, "avg_neg_vertical_speed", "m/s", 1000, 0, 0, 0, 61, 131),
           new globalField( null, null, "max_pos_vertical_speed", "m/s", 1000, 0, 0, 0, 62, 131),
           new globalField( null, null, "max_neg_vertical_speed", "m/s", 1000, 0, 0, 0, 63, 131),
           new globalField( null, null, "min_heart_rate", "bpm", 1, 0, 0, 0, 64, 2),
        };
        static public globalField[] lapFields = {
           new globalField( null, null, "message_index", "", 1, 0, 0, 0, 254, 132),
           new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400),
           new globalField( null, null, "event", "", 1, 0, 0, 0, 0, 0),
           new globalField( null, null, "event_type", "", 1, 0, 0, 0, 1, 0),
           new globalField( null, null, "start_time", "", 1, 0, 0, 0, 2, 134),
           new globalField( null, null, "start_position_lat", "semicircles", 1, 0, 0, 0, 3, 133),
           new globalField( null, null, "start_position_long", "semicircles", 1, 0, 0, 0, 4, 133),
           new globalField( null, null, "end_position_lat", "semicircles", 1, 0, 0, 0, 5, 133),
           new globalField( null, null, "end_position_long", "semicircles", 1, 0, 0, 0, 6, 133),
           new globalField( null, null, "total_elapsed_time", "s", 1000, 0, 0, 0, 7, 134),
           new globalField( null, null, "total_timer_time", "s", 1000, 0, 0, 0, 8, 134),
           new globalField( null, null, "total_distance", "m", 100, 0, 0, 0, 9, 134),
           new globalField( null, lapTotalCyclesSubFields, "total_cycles", "cycles", 1, 0, 0, 1, 10, 134),
           new globalField( null, null, "total_calories", "kcal", 1, 0, 0, 0, 11, 132),
           new globalField( null, null, "total_fat_calories", "kcal", 1, 0, 0, 0, 12, 132),
           new globalField( null, null, "avg_speed", "m/s", 1000, 0, 0, 0, 13, 132),
           new globalField( null, null, "max_speed", "m/s", 1000, 0, 0, 0, 14, 132),
           new globalField( null, null, "avg_heart_rate", "bpm", 1, 0, 0, 0, 15, 2),
           new globalField( null, null, "max_heart_rate", "bpm", 1, 0, 0, 0, 16, 2),
           new globalField( null, lapAvgCadenceSubFields, "avg_cadence", "rpm", 1, 0, 0, 1, 17, 2),
           new globalField( null, lapMaxCadenceSubFields, "max_cadence", "rpm", 1, 0, 0, 1, 18, 2),
           new globalField( null, null, "avg_power", "watts", 1, 0, 0, 0, 19, 132),
           new globalField( null, null, "max_power", "watts", 1, 0, 0, 0, 20, 132),
           new globalField( null, null, "total_ascent", "m", 1, 0, 0, 0, 21, 132),
           new globalField( null, null, "total_descent", "m", 1, 0, 0, 0, 22, 132),
           new globalField( null, null, "intensity", "", 1, 0, 0, 0, 23, 0),
           new globalField( null, null, "lap_trigger", "", 1, 0, 0, 0, 24, 0),
           new globalField( null, null, "sport", "", 1, 0, 0, 0, 25, 0),
           new globalField( null, null, "event_group", "", 1, 0, 0, 0, 26, 2),
           new globalField( null, null, "num_lengths", "lengths", 1, 0, 0, 0, 32, 132),
           new globalField( null, null, "normalized_power", "watts", 1, 0, 0, 0, 33, 132),
           new globalField( null, null, "left_right_balance", "", 1, 0, 0, 0, 34, 132),
           new globalField( null, null, "first_length_index", "", 1, 0, 0, 0, 35, 132),
           new globalField( null, null, "avg_stroke_distance", "m", 100, 0, 0, 0, 37, 132),
           new globalField( null, null, "swim_stroke", "", 1, 0, 0, 0, 38, 0),
           new globalField( null, null, "sub_sport", "", 1, 0, 0, 0, 39, 0),
           new globalField( null, null, "num_active_lengths", "lengths", 1, 0, 0, 0, 40, 132),
           new globalField( null, null, "total_work", "J", 1, 0, 0, 0, 41, 134),
           new globalField( null, null, "avg_altitude", "m", 5, 500, 0, 0, 42, 132),
           new globalField( null, null, "max_altitude", "m", 5, 500, 0, 0, 43, 132),
           new globalField( null, null, "gps_accuracy", "m", 1, 0, 0, 0, 44, 2),
           new globalField( null, null, "avg_grade", "%", 100, 0, 0, 0, 45, 131),
           new globalField( null, null, "avg_pos_grade", "%", 100, 0, 0, 0, 46, 131),
           new globalField( null, null, "avg_neg_grade", "%", 100, 0, 0, 0, 47, 131),
           new globalField( null, null, "max_pos_grade", "%", 100, 0, 0, 0, 48, 131),
           new globalField( null, null, "max_neg_grade", "%", 100, 0, 0, 0, 49, 131),
           new globalField( null, null, "avg_temperature", "C", 1, 0, 0, 0, 50, 1),
           new globalField( null, null, "max_temperature", "C", 1, 0, 0, 0, 51, 1),
           new globalField( null, null, "total_moving_time", "s", 1000, 0, 0, 0, 52, 134),
           new globalField( null, null, "avg_pos_vertical_speed", "m/s", 1000, 0, 0, 0, 53, 131),
           new globalField( null, null, "avg_neg_vertical_speed", "m/s", 1000, 0, 0, 0, 54, 131),
           new globalField( null, null, "max_pos_vertical_speed", "m/s", 1000, 0, 0, 0, 55, 131),
           new globalField( null, null, "max_neg_vertical_speed", "m/s", 1000, 0, 0, 0, 56, 131),
           new globalField( null, null, "min_heart_rate", "bpm", 1, 0, 0, 0, 63, 2),
        };
        static public globalField[] recordFields = {
            new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400),
            new globalField( null, null, "position_lat", "semicircles", 1, 0, 0, 0, 0, 133,100000000),
            new globalField( null, null, "position_long", "semicircles", 1, 0, 0, 0, 1, 133,100000000),
            new globalField( null, null, "altitude", "m", 5, 500, 0, 0, 2, 132),
            new globalField( null, null, "heart_rate", "bpm", 1, 0, 0, 0, 3, 2),
            new globalField( null, null, "cadence", "rpm", 1, 0, 0, 0, 4, 2),
            new globalField( null, null, "distance", "m", 100, 0, 0, 0, 5, 134),
            new globalField( null, null, "speed", "m/s", 1000, 0, 0, 0, 6, 132),
            new globalField( null, null, "power", "watts", 1, 0, 0, 0, 7, 132,2000),
            new globalField( recordCompressedSpeedDistanceComponents, null, "compressed_speed_distance", "", 1, 0, 2, 0, 8, 13),
            new globalField( null, null, "grade", "%", 100, 0, 0, 0, 9, 131),
            new globalField( null, null, "resistance", "", 1, 0, 0, 0, 10, 2),
            new globalField( null, null, "time_from_course", "s", 1000, 0, 0, 0, 11, 133),
            new globalField( null, null, "cycle_length", "m", 100, 0, 0, 0, 12, 2),
            new globalField( null, null, "temperature", "C", 1, 0, 0, 0, 13, 1,50),
            new globalField( null, null, "speed_1s", "m/s", 16, 0, 0, 0, 17, 2),
            new globalField( recordCyclesComponents, null, "cycles", "", 1, 0, 1, 0, 18, 2),
            new globalField( null, null, "total_cycles", "cycles", 1, 0, 0, 0, 19, 134),
            new globalField( recordCompressedAccumulatedPowerComponents, null, "compressed_accumulated_power", "", 1, 0, 1, 0, 28, 132),
            new globalField( null, null, "accumulated_power", "watts", 1, 0, 0, 0, 29, 134),
            new globalField( null, null, "left_right_balance", "", 1, 0, 0, 0, 30, 2),
            new globalField( null, null, "gps_accuracy", "m", 1, 0, 0, 0, 31, 2),
            new globalField( null, null, "vertical_speed", "m/s", 1000, 0, 0, 0, 32, 131),
            new globalField( null, null, "calories", "kcal", 1, 0, 0, 0, 33, 132),
        };
        static public globalField[] eventFields = {
            new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400),
            new globalField( null, null, "event", "", 1, 0, 0, 0, 0, 0,50),
            new globalField( null, null, "event_type", "", 1, 0, 0, 0, 1, 0,15),
            new globalField( eventData16Components,null, "data16", "", 1, 0, 1, 0, 2, 132),
            new globalField( null, eventDataSubFields, "data", "", 1, 0, 0, 16, 3, 134),
            new globalField( null, null, "event_group", "", 1, 0, 0, 0, 4, 2),
        };
        static public globalField[] sourceFields =
        {
            new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400),
            new globalField( null, null, "Speed", "", 1, 0, 0, 0, 0, 2),
            new globalField( null, null, "Distance", "", 1, 0, 0, 0, 1, 2),
            new globalField( null, null, "Cadence", "", 1, 0, 0, 0, 2, 2),
            new globalField( null, null, "Altitude", "", 1, 0, 0, 0, 3, 2),
            new globalField( null, null, "HeartRate", "", 1, 0, 0, 0, 4, 2),
            new globalField( null, null, "Calories", "", 1, 0, 0, 0, 5, 2),
            new globalField( null, null, "Power", "", 1, 0, 0, 0, 6, 2),
            new globalField( null, null, "?", "", 1, 0, 0, 0, 7, 2),
            new globalField( null, null, "?", "", 1, 0, 0, 0, 8, 2),
        };
        static public globalField[] deviceInfoFields =
        {
           new globalField( null, null, "timestamp", "s", 1, 0, 0, 0, 253, 134, 2678400),
           new globalField( null, null, "device_index", "", 1, 0, 0, 0, 0, 2,10),
           new globalField( null, null, "device_type", "", 1, 0, 0, 0, 1, 2),
           new globalField( null, null, "manufacturer", "", 1, 0, 0, 0, 2, 132,6000),
           new globalField( null, null, "serial_number", "", 1, 0, 0, 0, 3, 140),
           new globalField( null, null, "product", "", 1, 0, 0, 0, 4, 132),
           new globalField( null, null, "software_version", "", 100, 0, 0, 0, 5, 132),
           new globalField( null, null, "hardware_version", "", 1, 0, 0, 0, 6, 2),
           new globalField( null, null, "cum_operating_time", "s", 1, 0, 0, 0, 7, 134),
           new globalField( null, null, "Onbekend", "", 1, 0, 0, 0, 8, 134),
           new globalField( null, null, "battery_voltage", "V", 256, 0, 0, 0, 10, 132),
           new globalField( null, null, "battery_status", "", 1, 0, 0, 0, 11, 2),
        };
        static public globalField[] workoutFields;
        static public globalField[] workoutStepFields;
        static public globalField[] scheduleFields;
        static public globalField[] weightScaleFields;
        static public globalField[] courseFields;
        static public globalField[] coursePointFields;
        static public globalField[] totalsFields;
        static public globalField[] activityFields =
        {
            new globalField(null,null,"timestamp", "", 1, 0, 0, 0, 253, 134 ),
            new globalField(null,null,"total_timer_time", "s", 1000, 0, 0, 0, 0, 134 ),
            new globalField(null,null,"num_sessions", "", 1, 0, 0, 0, 1, 132 ),
            new globalField(null,null,"type", "", 1, 0, 0, 0, 2, 0 ),
            new globalField(null,null,"event", "", 1, 0, 0, 0, 3, 0 ),
            new globalField(null,null,"event_type", "", 1, 0, 0, 0, 4, 0 ),
            new globalField(null,null,"local_timestamp", "", 1, 0, 0, 0, 5, 134 ),
            new globalField(null,null,"event_group", "", 1, 0, 0, 0, 6, 2 ),
        };
        static public globalField[] softwareFields;
        static public globalField[] fileCapabilitiesFields;
        static public globalField[] mesgCapabilitiesFields;
        static public globalField[] fieldCapabilitiesFields;
        static public globalField[] fileCreatorFields = {
            new globalField(null,null, "software_version", "", 1, 0, 0, 0, 0, 132,0 ),
            new globalField(null,null, "hardware_version", "", 1, 0, 0, 0, 1, 2,0 )
        };
        static public globalField[] bloodPressureFields;
        static public globalField[] monitoringFields;
        static public globalField[] hrvFields;
        static public globalField[] lengthFields;
        static public globalField[] monitoringInfoFields;

        public static MESG[] mesgs = 
        { 
           new MESG(fileIdFields, "file_id", FIT_MESG_NUM_FILE_ID, 6),
           new MESG(capabilitiesFields, "capabilities", FIT_MESG_NUM_CAPABILITIES, 3),
           new MESG(deviceSettingsFields, "device_settings", FIT_MESG_NUM_DEVICE_SETTINGS, 1),
           new MESG(userProfileFields, "user_profile", FIT_MESG_NUM_USER_PROFILE, 22),
           new MESG(hrmProfileFields, "hrm_profile", FIT_MESG_NUM_HRM_PROFILE, 5),
           new MESG(sdmProfileFields, "sdm_profile", FIT_MESG_NUM_SDM_PROFILE, 7),
           new MESG(bikeProfileFields, "bike_profile", FIT_MESG_NUM_BIKE_PROFILE, 26),
           new MESG(zonesTargetFields, "zones_target", FIT_MESG_NUM_ZONES_TARGET, 5),
           new MESG(hrZoneFields, "hr_zone", FIT_MESG_NUM_HR_ZONE, 3),
           new MESG(powerZoneFields, "power_zone", FIT_MESG_NUM_POWER_ZONE, 3),
           new MESG(metZoneFields, "met_zone", FIT_MESG_NUM_MET_ZONE, 4),
           new MESG(sportFields, "sport", FIT_MESG_NUM_SPORT, 3),
           new MESG(goalFields, "goal", FIT_MESG_NUM_GOAL, 12),
           new MESG(sessionFields, "session", FIT_MESG_NUM_SESSION, 61),
           new MESG(lapFields, "lap", FIT_MESG_NUM_LAP, 54),
           new MESG(recordFields, "record", FIT_MESG_NUM_RECORD, 24),
           new MESG(eventFields, "event", FIT_MESG_NUM_EVENT, 6),
           new MESG(sourceFields, "source", FIT_MESG_NUM_SOURCE, 10),
           new MESG(deviceInfoFields, "device_info", FIT_MESG_NUM_DEVICE_INFO, 11),
           new MESG(workoutFields, "workout", FIT_MESG_NUM_WORKOUT, 4),
           new MESG(workoutStepFields, "workout_step", FIT_MESG_NUM_WORKOUT_STEP, 9),
           new MESG(scheduleFields, "schedule", FIT_MESG_NUM_SCHEDULE, 7),
           new MESG(weightScaleFields, "weight_scale", FIT_MESG_NUM_WEIGHT_SCALE, 13),
           new MESG(courseFields, "course", FIT_MESG_NUM_COURSE, 3),
           new MESG(coursePointFields, "course_point", FIT_MESG_NUM_COURSE_POINT, 7),
           new MESG(totalsFields, "totals", FIT_MESG_NUM_TOTALS, 8),
           new MESG(activityFields, "activity", FIT_MESG_NUM_ACTIVITY, 8),
           new MESG(softwareFields, "software", FIT_MESG_NUM_SOFTWARE, 5),
           new MESG(fileCapabilitiesFields, "file_capabilities", FIT_MESG_NUM_FILE_CAPABILITIES, 6),
           new MESG(mesgCapabilitiesFields, "mesg_capabilities", FIT_MESG_NUM_MESG_CAPABILITIES, 5),
           new MESG(fieldCapabilitiesFields, "field_capabilities", FIT_MESG_NUM_FIELD_CAPABILITIES, 5),
           new MESG(fileCreatorFields, "file_creator", FIT_MESG_NUM_FILE_CREATOR, 2),
           new MESG(bloodPressureFields, "blood_pressure", FIT_MESG_NUM_BLOOD_PRESSURE, 11),
           new MESG(monitoringFields, "monitoring", FIT_MESG_NUM_MONITORING, 12),
           new MESG(hrvFields, "hrv", FIT_MESG_NUM_HRV, 1),
           new MESG(lengthFields, "length", FIT_MESG_NUM_LENGTH, 14),
           new MESG(monitoringInfoFields, "monitoring_info", FIT_MESG_NUM_MONITORING_INFO, 2),
           new MESG(null, "pad", FIT_MESG_NUM_PAD, 0),
        };
    }
}
