using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace myFit
{
    public class localField
    {
        public byte FieldDefinitionNumber;
        public byte size;
        public byte baseType;
        public int arch;
        public Int64 lastGoodValue = Int64.MinValue;
        public globalField GlobalField;
    }
    public class localMsgDef
    {
        public bool isValid = false;
        public bool isCompressedMsg = true;
        public byte reserved;
        public byte Architecture;
        public UInt16 globalMesgIndex;
        public MESG mesg;
        public byte Fields;
        public localField[] localFields;
    }

    public class fitFileReader
    {
        public fitFileReader(FileStream file, StreamWriter outFile)
        {
            string ErrorBytes = "";
            int RecordsOk = 5;
            localMsgDef[] localMesgDefs = new localMsgDef[16];
            FitRecord record = new FitRecord(file);

            long pos = file.Position = (byte)file.ReadByte() == 14 ? 14 : 12;
            UInt32 TraveltotalSeconds = 0;
            UInt32 TravelStartTimestamp = 0;
            UInt32 TravelEndTimestamp = 0;
            UInt32 TraveltotalMeters = 0;
            UInt32 TravelStartMeters = 0;
            UInt32 TravelEndMeters = 0;
            UInt32 maxSpeed = 0;
            UInt32 totPower = 0;
            UInt32 PowertotalSeconds = 0;
            UInt32 PowerStartTimestamp = 0;
            UInt32 PowerEndTimestamp = 0;


            while (true)
            {
                try
                {
                    if (RecordsOk > 5)
                        pos = file.Position;
                    string str = record.read(localMesgDefs);
                    if (str == "")
                    {
                        if (pos + 2 >= file.Length) break;
                        file.Position = pos++;
                        ErrorBytes += ((RecordsOk > 0) ? string.Format("Error at pos {0} byte:", (file.Position).ToString("X4")) : "") + file.ReadByte().ToString("X2");
                        RecordsOk = 0;
                    }
                    else
                    {
                        RecordsOk++;
                        if (ErrorBytes != "") outFile.WriteLine(ErrorBytes);
                        ErrorBytes = "";
                        outFile.WriteLine(str);
                        if (record.localMsgDef.globalMesgIndex == FIT.FIT_MESG_NUM_RECORD && record.speed > 2000)
                        {
                            TravelEndTimestamp = record.timestamp;
                            TravelEndMeters = record.distance;
                            if (TravelStartMeters == 0)
                            {
                                TravelStartTimestamp = TravelEndTimestamp;
                                TravelStartMeters = TravelEndMeters;
                            }
                        }
                        else
                        {
                            TraveltotalSeconds += TravelEndTimestamp - TravelStartTimestamp;
                            TraveltotalMeters += TravelEndMeters - TravelStartMeters;
                            TravelStartTimestamp = TravelEndTimestamp = 0;
                            TravelStartMeters = TravelEndMeters = 0;
                        }
                        if (record.localMsgDef.globalMesgIndex == FIT.FIT_MESG_NUM_RECORD && record.power > 0 && record.power != 0xFFFF)
                        {
                            PowerEndTimestamp = record.timestamp;
                            totPower += record.power;
                            if (PowerStartTimestamp == 0)
                            {
                                PowerStartTimestamp = PowerEndTimestamp;
                            }
                        }
                        else
                        {
                            PowertotalSeconds += PowerEndTimestamp - PowerStartTimestamp;
                            PowerStartTimestamp = PowerEndTimestamp = 0;
                        }

                    }
                }
                catch (IOException e) { break; }
                catch (Exception ex) { Console.WriteLine(ex.Message); ; }
            }
            closeSession(outFile, localMesgDefs, TravelEndTimestamp);
            if (ErrorBytes != "") outFile.WriteLine(ErrorBytes);
            outFile.WriteLine(String.Format("Tijd: {0}, Afstand: {1}, Snelheid: {2}. Gem Vermogen: {3}, Calorien: {4}", new TimeSpan(0, 0, (int)TraveltotalSeconds), (TraveltotalMeters / 100000.0).ToString("0.00"), ((TraveltotalMeters / 100000.0) / (TraveltotalSeconds / 3600.0)).ToString("0.0"), PowertotalSeconds == 0 ? 0 : totPower / PowertotalSeconds, totPower / 1000.0 * 1.1));
        }
        private void closeSession(StreamWriter outFile, localMsgDef[] localMsgDefs, UInt32 TravelEndTimestamp)//, DateTime dt, UInt32 start_position_lat, UInt32 start_position_long, UInt32 total_elapsed_time, UInt32 total_timer_time, UInt32 total_distance, UInt32 total_calories, UInt32 avg_speed, UInt32 max_speed, UInt32 avg_power, UInt32 max_power, UInt32 total_ascent, UInt32 total_descent, UInt32 avg_heart_rate, UInt32 max_heart_rate, UInt32 avg_cadence, UInt32 max_cadence)
        {
            int _event = 0;
            int i = 0;
            for (i = 0; i < 16; i++)
            {

                if (localMsgDefs[i] == null) break;
                if (localMsgDefs[i].globalMesgIndex == FIT.FIT_MESG_NUM_EVENT) _event = i;
                if (localMsgDefs[i].globalMesgIndex == FIT.FIT_MESG_NUM_SESSION) return;
            }
            outFile.WriteLine(string.Format("def;{0};0;0;18;31;253;4;134;2;4;134;3;4;133;4;4;133;7;4;134;8;4;134;9;4;134;10;4;134;29;4;133;30;4;133;31;4;133;32;4;133;254;2;132;11;2;132;13;2;132;14;2;132;15;2;132;20;2;132;21;2;132;22;2;132;23;2;132;25;2;132;26;2;132;0;1;0;1;1;0;5;1;0;6;1;0;16;1;2;17;1;2;18;1;2;19;1;2", i + 64));
            outFile.WriteLine(string.Format("data;{0};{1};708788214;627355257;56710458;6176,85;5831,27;52979,47;;;;;;0;997;;9,085;13,627;231;712;137;133;0;53;SESSION;STOP;CYCLING;255;154;180;76;116", i, FIT.timestampToLocalTime(TravelEndTimestamp)));
            outFile.WriteLine(string.Format("data;{0};{1};1;SESSION;STOP_DISABLE_ALL;1", _event, FIT.timestampToLocalTime(TravelEndTimestamp)));
            outFile.WriteLine(string.Format("def;{0};0;0;34;6;253;4;134;0;4;134;1;2;132;2;1;0;3;1;0;4;1;0", i + 1 + 64));
            outFile.WriteLine(string.Format("data;{0};{1};5831,27;1;0;ACTIVITY;STOP", i + 1, FIT.timestampToLocalTime(TravelEndTimestamp)));
        }
        private void readFileHeader(FileStream file)
        {
            int fileDataSize = 0;
            int fileHdrSize = 0;
            string protocolVersion = "";
            Int64 profileVersion = 0;
            string FIT = null;
            fileHdrSize = file.ReadByte();
            byte data = (byte)file.ReadByte();
            protocolVersion = string.Format("{0}.{1}", data & 0xF0, data & 0x0F);
            profileVersion = file.ReadByte() + (file.ReadByte() << 8);
            fileDataSize = file.ReadByte() + (file.ReadByte() << 8) + (file.ReadByte() << 16) + (file.ReadByte() << 24);
            FIT = string.Format("{0}{1}{2}{3}", (char)file.ReadByte(), (char)file.ReadByte(), (char)file.ReadByte(), (char)file.ReadByte());
        }

        public class FitRecord
        {
            string recordDefinitionHeader = "";
            string recordString = "";
            public UInt32 timestamp = 0;
            public Int32 position_lat;
            public Int32 position_long;
            public UInt16 altitude;
            public UInt32 distance;
            public UInt16 speed;
            public UInt16 power;
            public int _event;
            public int _event_type;
            byte cadence;
            int localMesgIndex = 0;
            int timeOffset = 0;
            int lastTimeOffset = 0;
            bool error = false;
            enum recordtype { data, definition, invalid };
            localMsgDef[] localMsgDefs;
            public localMsgDef localMsgDef;
            FitFieldStream file;
            public FitRecord(FileStream file)
            {
                this.file = new FitFieldStream(file);
            }

            public string read(localMsgDef[] lMesgDefs)
            {
                this.localMsgDefs = lMesgDefs;
                switch (readRecordHeader())
                {
                    case recordtype.definition:
                        return readFieldDef();
                    case recordtype.data:
                        return readFieldData();
                    case recordtype.invalid:
                        error = true;
                        return "";
                    default:
                        throw new Exception("recordtype onbekend");
                }
            }

            private recordtype readRecordHeader()
            {
                recordtype recordType;
                error = false;
                byte header = file.readbyte();
                if ((header & 0x80) != 0) //compres time field in data record
                {
                    recordType = recordtype.data;
                    localMesgIndex = ((int)header & 0x60) >> 5;
                    timeOffset = (int)header & 0x1f;
                    timestamp += (UInt16)((timeOffset - lastTimeOffset) & 0x1f);
                    lastTimeOffset = timeOffset;
                    if (localMsgDefs[localMesgIndex] == null || !localMsgDefs[localMesgIndex].isValid || !localMsgDefs[localMesgIndex].isCompressedMsg) return recordtype.invalid;
                    recordString = (string.Format("CompData:{0};{1}", localMesgIndex, header));
                }
                else if ((header & 0x40) != 0)
                {
                    recordType = recordtype.definition;
                    localMesgIndex = (int)header & 0x0F;
                    if (localMesgIndex > 0 && (localMsgDefs[localMesgIndex - 1] == null || !localMsgDefs[localMesgIndex - 1].isValid)) return recordtype.invalid;
                    if ((header & 0x30) != 0) return recordtype.invalid;
                    recordString = (string.Format("def;{0}", header));
                }
                else
                {
                    recordType = recordtype.data;
                    localMesgIndex = (int)header & 0x0F;
                    if (localMsgDefs[localMesgIndex] == null) return recordtype.invalid;
                    if ((header & 0x30) != 0) return recordtype.invalid;
                    recordString = (string.Format("data;{0}", header));
                }
                return recordType;
            }
            private string readFieldData()
            {
                string str = "";
                localMsgDef = localMsgDefs[localMesgIndex];
                foreach (localField localField in localMsgDef.localFields)
                {
                    str = file.readValue(localField);
                    globalField globalField = localField.GlobalField;
                    if (globalField.foutMarge >= 0)
                    {
                        Int64 value = file.getLastValue();
                        if (file.isLastValueValid)
                            if (localField.lastGoodValue == Int64.MinValue)
                                localField.lastGoodValue = value;
                            else
                                if (Math.Abs(localField.lastGoodValue - value) > globalField.foutMarge)
                                    return "";
                                else
                                    localField.lastGoodValue = value;
                        else
                        {
                            if (globalField.name == "software_version" || globalField.name == "device_index") //verplicht veld
                                return "";
                        }
                    }

                    if (globalField.name == "timestamp") str = FIT.timestampToLocalTime(UInt32.Parse(str));
                    recordString += ";" + str;
                    switch (globalField.name)
                    {
                        case "timestamp": timestamp = file._Uint32; break;
                        case "position_lat": position_lat = file._int32; break;
                        case "position_long": position_long = file._int32; break;
                        case "altitude": altitude = file._Uint16; break;
                        case "distance": distance = file._Uint32; break;
                        case "speed": speed = file._Uint16; break;
                        case "power": power = file._Uint16; break;
                        case "cadence": cadence = file._byte; break;
                        case "event": _event = file._enum; break;
                        case "event_type": _event_type = file._enum; break;
                    }
                }
                return recordString;
            }
            private string readFieldDef()
            {
                localMsgDef msgDef = new localMsgDef();

                msgDef.reserved = file.readbyte();
                if (msgDef.reserved != 0) return "";

                msgDef.Architecture = file.readbyte();
                if (msgDef.Architecture > 1) return "";

                msgDef.globalMesgIndex = file.readUInt16(msgDef.Architecture);
                if (msgDef.globalMesgIndex > 255 && msgDef.globalMesgIndex < 0xFE00) return "";
                msgDef.mesg = FIT.getMessageStruct(msgDef.globalMesgIndex);

                msgDef.Fields = file.readbyte();
                if (msgDef.Fields > msgDef.mesg.fields.Length + 10) return "";

                recordString += (string.Format(";{0};{1};{2};{3}", msgDef.reserved, msgDef.Architecture, msgDef.globalMesgIndex, msgDef.Fields));
                recordDefinitionHeader = ";;reserved;Architecture;globalMesgIndex;Fields";
                msgDef.localFields = new localField[msgDef.Fields];
                for (int i = 0; i < msgDef.Fields; i++)
                {
                    localField fieldDef = new localField();
                    byte d = fieldDef.FieldDefinitionNumber = file.readbyte();
                    byte s = fieldDef.size = file.readbyte();
                    byte b = fieldDef.baseType = file.readbyte();
                    fieldDef.arch = (((b & 0x80) == 0x80) && msgDef.Architecture == 1) ? 1 : 0; ;
                    fieldDef.GlobalField = FIT.getFieldStruct(msgDef.mesg, d);
                    {
                        if (d == 253) msgDef.isCompressedMsg = false;
                        //if (fieldDef.field.valid && (fieldDef.field.type != b || fieldDef.field.num != d)) return "";
                        if (!(d <= 80 || d >= 253)) return "";
                        if (!(b == 0x00 && s == 1 || b == 0x01 && s == 1 || b == 0x02 && s == 1 || b == 0x83 && s == 2 || b == 0x84 && s == 2 || b == 0x85 && s == 4 || b == 0x86 && s == 4 || b == 0x07 && s > 0 || b == 0x88 && s == 4 || b == 0x89 && s == 8 || b == 0x0A && s == 1 || b == 0x8B && s == 2 || b == 0x8C && s == 4 || b == 0x0D && s > 0)) return "";
                        recordString += (string.Format(";{0};{1};{2}", d, s, b));
                    }
                    msgDef.localFields[i] = fieldDef;
                }
                msgDef.isValid = true;
                localMsgDef = localMsgDefs[localMesgIndex] = msgDef;
                return string.Format("{0}\n{1}\n{2}", recordDefinitionHeader, recordString, getHeader(localMesgIndex));
            }
            private string getHeader(int localMsgIndex)
            {
                string s = string.Format("{0};{1}", localMsgDefs[localMsgIndex].mesg.name, localMsgIndex);
                int globalMesgIndex = localMsgDefs[localMsgIndex].globalMesgIndex;
                foreach (localField f in localMsgDefs[localMsgIndex].localFields)
                    s += string.Format(";{0}({1})", f.GlobalField.name, f.GlobalField.units).Replace("()", "");
                return s;
            }

        }

        public class FitFieldStream
        {
            FileStream file;
            public int _enum;
            public byte _byte;
            sbyte _sbyte;
            public Int16 _int16;
            public UInt16 _Uint16;
            public Int32 _int32;
            public UInt32 _Uint32;
            UInt64 _Uint64;

            string strValue;
            int baseType;
            public bool isLastValueValid;

            public bool eof { get { return (file.Position + 2 > file.Length); } }
            public FitFieldStream(FileStream file)
            {
                this.file = file;
            }
            public Int64 getLastValue()
            {
                switch (baseType)
                {
                    case 0x00: return (Int64)_enum;//enum
                    case 0x01: return (Int64)_sbyte; //sbyte
                    case 0x02: return (Int64)_byte;//byte
                    case 0x83: return (Int64)_int16; //int16
                    case 0x84: return (Int64)_Uint16;//Uint16
                    case 0x85: return (Int64)_int32; //int32
                    case 0x86: return (Int64)_Uint32; //Uint32
                    case 0x87: return (Int64)_Uint32; //float32
                    case 0x89: return (Int64)_Uint64; //float64
                    case 0x0A: return (Int64)_byte; //byteZ
                    case 0x8B: return (Int64)_Uint16;//Uint16Z
                    case 0x8C: return (Int64)_Uint32;//Uint32Z
                    default:
                        return 0;
                }
            }
            public string readValue(localField localFieldDef)
            {
                globalField FIELD = localFieldDef.GlobalField;
                double scale = FIELD.scale;
                double offset = FIELD.offset;
                int size = localFieldDef.size;
                int arch = localFieldDef.arch;
                string fm = scale <= 1 ? "0" : scale <= 10 ? "0.0" : scale <= 100 ? "0.00" : scale <= 1000 ? "0.000" : "0.0000";
                baseType = localFieldDef.baseType;
                switch (baseType)
                {
                    case 0x00: strValue = FIT.EnumToString(localFieldDef.GlobalField.name, (int)(_enum = readbyte())); break;//enum
                    case 0x01: strValue = ((isLastValueValid = (_sbyte = readsbyte()) != 0x7f) ? (_sbyte / scale - offset).ToString(fm) : ""); break;//sbyte
                    case 0x02: strValue = ((isLastValueValid = (_byte = readbyte()) != 0xFF) ? (_byte / scale - offset).ToString(fm) : ""); break;//byte
                    case 0x83: strValue = ((isLastValueValid = (_int16 = readInt16(arch)) != 0x7FFF) ? (_int16 / scale - offset).ToString(fm) : ""); break;//int16
                    case 0x84: strValue = ((isLastValueValid = (_Uint16 = readUInt16(arch)) != 0xFFFF) ? (_Uint16 / scale - offset).ToString(fm) : ""); break;//Uint16
                    case 0x85: strValue = ((isLastValueValid = (_int32 = readInt32(arch)) != 0x7FFFFFFF) ? (_int32 / scale - offset).ToString(fm) : ""); break;//int32
                    case 0x86: strValue = ((isLastValueValid = (_Uint32 = readUInt32(arch)) != 0xFFFFFFFF) ? (_Uint32 / scale - offset).ToString(fm) : ""); break;//Uint32
                    case 0x87: strValue = ((isLastValueValid = (_Uint32 = readUInt32(arch)) != 0xFFFFFFFF) ? (_Uint32 / scale - offset).ToString(fm) : ""); break;//float32
                    case 0x89: strValue = ((isLastValueValid = (_Uint64 = readUInt64(arch)) != 0xFFFFFFFFFFFFFFFF) ? (_Uint64 / scale - offset).ToString(fm) : ""); break;//float64
                    case 0x0A: strValue = ((isLastValueValid = (_byte = readbyte()) != 0x00) ? (_byte / scale - offset).ToString(fm) : ""); break;//byteZ
                    case 0x8B: strValue = ((isLastValueValid = (_Uint16 = readUInt16(arch)) != 0x00) ? (_Uint16 / scale - offset).ToString(fm) : ""); break;//Uint16Z
                    case 0x8C: strValue = ((isLastValueValid = (_Uint32 = readUInt32(arch)) != 0x00) ? (_Uint32 / scale - offset).ToString(fm) : ""); break; ;//Uint32Z
                    case 0x07:
                        for (int i = 0; i < size; i++)
                            strValue += readChar();
                        break; ; //string
                    case 0x0D:
                        for (int i = 0; i < size; i++)
                            strValue += readbyte().ToString("X2");
                        break; ; //string
                    default:
                        break;
                }
                switch (FIELD.name)
                {
                    case "timestamp": break;
                    case "position_lat": break;
                    case "position_long": break;
                    case "altitude": break;
                    case "distance": break;
                    case "speed": break;
                    case "power": break;
                }

                return strValue;
            }
            public char readChar()
            {
                return (char)file.ReadByte();
            }
            public byte readbyte()
            {
                return (byte)file.ReadByte();
            }
            private sbyte readsbyte()
            {
                return (sbyte)file.ReadByte();
            }
            private Int64 readInt()
            {
                return file.ReadByte() + (file.ReadByte() << 8) + (file.ReadByte() << 16) + (file.ReadByte() << 24);
            }
            public Int16 readInt16(int arch)
            {
                return (Int16)((arch == 0) ? file.ReadByte() | file.ReadByte() << 8 : file.ReadByte() << 8 | file.ReadByte());
            }
            public UInt16 readUInt16(int arch)
            {
                return (UInt16)((arch == 0) ? file.ReadByte() | file.ReadByte() << 8 : file.ReadByte() << 8 | file.ReadByte());
            }
            public Int32 readInt32(int arch)
            {
                return (Int32)((arch == 0) ? file.ReadByte() | file.ReadByte() << 8 | file.ReadByte() << 16 | file.ReadByte() << 24 : file.ReadByte() << 24 | file.ReadByte() << 16 | file.ReadByte() << 8 | file.ReadByte());
            }
            private UInt32 readUInt32(int arch)
            {
                return (UInt32)((arch == 0) ? file.ReadByte() | file.ReadByte() << 8 | file.ReadByte() << 16 | file.ReadByte() << 24 : file.ReadByte() << 24 | file.ReadByte() << 16 | file.ReadByte() << 8 | file.ReadByte());
            }
            private UInt64 readUInt64(int arch)
            {
                return (UInt64)((arch == 0) ? file.ReadByte() | file.ReadByte() << 8 | file.ReadByte() << 16 | file.ReadByte() << 24 | file.ReadByte() << 32 | file.ReadByte() << 48 | file.ReadByte() << 56 | file.ReadByte() << 64 : file.ReadByte() << 64 | file.ReadByte() << 56 | file.ReadByte() << 48 | file.ReadByte() << 32 | file.ReadByte() << 24 | file.ReadByte() << 16 | file.ReadByte() << 8 | file.ReadByte());
            }
        }
    }

}
