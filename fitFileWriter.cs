using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace myFit
{
    public class statistics
    {
        public Int32 start_position_lat;
        public Int32 start_position_long;
        public UInt32 total_elapsed_time;
        public UInt32 total_timer_time;
        public UInt32 total_distance;
        public UInt16 total_calories;
        public UInt16 avg_speed;
        public UInt16 max_speed;
        public UInt16 avg_power;
        public UInt16 max_power;
        public byte avg_heart_rate;
        public byte max_heart_rate;
        public byte avg_cadence;
        public byte max_cadence;

        public UInt32 timestamp = 0;
        public UInt32 distance;
        public UInt16 speed = 0;
        public UInt16 power = 0;
        public UInt16 altitude = 0;
        public byte heart_rate = 0;
        public byte cadence = 0;

        UInt16 lastAltitude = 0xFFFF;
        UInt32 lastTimestamp = 0;
        UInt32 lastTravelCM = 0;

        public UInt16 SessionTotal_ascent;
        public UInt16 SessionTotal_descent;
        public UInt32 SessionStartTimestamp = 0XFFFFFFFF;
        UInt32 SessionTravelTotSeconds = 0;
        UInt32 SessionTravelStartTimestamp = 0;
        UInt32 SessionTravelTotcm = 0;
        UInt32 SessionTravelStartCM = 0;
        UInt32 SessionPowerTot = 0;
        UInt32 SessionPowerTotSeconds = 0;
        UInt32 SessionPowerStartTimestamp = 0;
        UInt32 SessionHeart_rateTot = 0;
        UInt32 SessionHeart_rateTotSeconds = 0;
        UInt32 SessionHeart_rateStartTimestamp = 0;
        UInt32 SessionCadenceTot = 0;
        UInt32 SessionCadenceTotSeconds = 0;
        UInt32 SessionCadenceStartTimestamp = 0;
        bool SessionEnd = false;

        public UInt16 LapTotal_ascent;
        public UInt16 LapTotal_descent;
        public UInt32 LapStartTimestamp = 0XFFFFFFFF;
        UInt32 LapTravelTotSeconds = 0;
        UInt32 LapTravelStartTimestamp = 0;
        UInt32 LapTravelTotcm = 0;
        UInt32 LapTravelStartCM = 0;
        UInt32 LapPowerTot = 0;
        UInt32 LapPowerTotSeconds = 0;
        UInt32 LapPowerStartTimestamp = 0;
        UInt32 LapHeart_rateTot = 0;
        UInt32 LapHeart_rateTotSeconds = 0;
        UInt32 LapHeart_rateStartTimestamp = 0;
        UInt32 LapCadenceTot = 0;
        UInt32 LapCadenceTotSeconds = 0;
        UInt32 LapCadenceStartTimestamp = 0;
        bool LapEnd = false;

        public void calcSession()
        {
            SessionEnd = true;
            update();
            SessionEnd = false;
            total_elapsed_time = (UInt32)((timestamp - SessionStartTimestamp) * 1000);
            total_timer_time = (UInt32)(SessionTravelTotSeconds * 1000);
            total_distance = distance;
            total_calories = (UInt16)(SessionPowerTot / 1000.0 * 1.1 + 0.1);
            avg_speed = (UInt16)((SessionTravelTotcm * 10.0) / (SessionTravelTotSeconds));
            avg_power = SessionPowerTotSeconds == 0 ? (UInt16)0xFFFF : (UInt16)(SessionPowerTot / SessionPowerTotSeconds);
            avg_heart_rate = SessionHeart_rateTotSeconds == 0 ? (byte)0xFF : (byte)(SessionHeart_rateTot / SessionHeart_rateTotSeconds);
            avg_cadence = SessionCadenceTotSeconds == 0 ? (byte)0xFF : (byte)(SessionCadenceTot / SessionCadenceTotSeconds);
        }
        public void resetSession()
        {
            SessionTotal_ascent = 0;
            SessionTotal_descent = 0;
            SessionStartTimestamp = 0xFFFFFFFF;
            SessionTravelTotSeconds = 0;
            SessionTravelStartTimestamp = 0;
            SessionTravelTotcm = 0;
            SessionTravelStartCM = 0;
            SessionPowerTot = 0;
            SessionPowerTotSeconds = 0;
            SessionPowerStartTimestamp = 0;
            SessionHeart_rateTot = 0;
            SessionHeart_rateTotSeconds = 0;
            SessionHeart_rateStartTimestamp = 0;
            SessionCadenceTot = 0;
            SessionCadenceTotSeconds = 0;
            SessionCadenceStartTimestamp = 0;
        }
        public void calcLap()
        {
            LapEnd = true;
            update();
            LapEnd = false;

            total_elapsed_time = (UInt32)((timestamp - LapStartTimestamp) * 1000);
            total_timer_time = (UInt32)(LapTravelTotSeconds * 1000);
            total_distance = LapTravelTotcm;
            total_calories = (UInt16)(LapPowerTot / 1000.0 * 1.1 + 0.1);
            avg_speed = (UInt16)((LapTravelTotcm * 10.0) / (LapTravelTotSeconds));
            avg_power = LapPowerTotSeconds == 0 ? (UInt16)0xFFFF : (UInt16)(LapPowerTot / LapPowerTotSeconds);
            avg_heart_rate = LapHeart_rateTotSeconds == 0 ? (byte)0xFF : (byte)(LapHeart_rateTot / LapHeart_rateTotSeconds);
            avg_cadence = LapCadenceTotSeconds == 0 ? (byte)0xFF : (byte)(LapCadenceTot / LapCadenceTotSeconds);

        }
        public void resetLap()
        {
            LapTotal_ascent = 0;
            LapTotal_descent = 0;
            LapStartTimestamp = 0xFFFFFFFF;
            LapTravelTotSeconds = 0;
            LapTravelStartTimestamp = 0;
            LapTravelTotcm = 0;
            LapTravelStartCM = 0;
            LapPowerTot = 0;
            LapPowerTotSeconds = 0;
            LapPowerStartTimestamp = 0;
            LapHeart_rateTot = 0;
            LapHeart_rateTotSeconds = 0;
            LapHeart_rateStartTimestamp = 0;
            LapCadenceTot = 0;
            LapCadenceTotSeconds = 0;
            LapCadenceStartTimestamp = 0;
        }
        public void update()
        {
            if (SessionStartTimestamp == 0xFFFFFFFF) SessionStartTimestamp = timestamp;
            if (LapStartTimestamp == 0xFFFFFFFF) LapStartTimestamp = timestamp;
            if (lastAltitude == 0xFFFF) lastAltitude = altitude;
            if (altitude - lastAltitude > 0)
            {
                SessionTotal_ascent += (UInt16)(altitude - lastAltitude);
                LapTotal_ascent += (UInt16)(altitude - lastAltitude);
            }
            if (lastAltitude - altitude > 0)
            {
                SessionTotal_descent += (UInt16)(lastAltitude - altitude);
                LapTotal_descent += (UInt16)(lastAltitude - altitude);
            }

            if (speed > 0 && !LapEnd && !SessionEnd)
            {
                if (speed > max_speed) max_speed = speed;
                if (SessionTravelStartCM == 0)
                {
                    SessionTravelStartTimestamp = timestamp;
                    SessionTravelStartCM = distance;
                }
                if (LapTravelStartCM == 0)
                {
                    LapTravelStartTimestamp = timestamp;
                    LapTravelStartCM = distance;
                }
            }
            else
            {
                if (SessionTravelStartCM != 0)
                {
                    SessionTravelTotSeconds += lastTimestamp - SessionTravelStartTimestamp;
                    SessionTravelTotcm += lastTravelCM - SessionTravelStartCM;
                    SessionTravelStartTimestamp = 0;
                    SessionTravelStartCM = 0;
                }
                if (LapTravelStartCM != 0)
                {
                    LapTravelTotSeconds += lastTimestamp - LapTravelStartTimestamp;
                    LapTravelTotcm += lastTravelCM - LapTravelStartCM;
                    LapTravelStartTimestamp = 0;
                    LapTravelStartCM = 0;
                }
            }
            if (power > 0 && !LapEnd && !SessionEnd)
            {
                if (power > max_power) max_power = power;
                SessionPowerTot += power;
                if (SessionPowerStartTimestamp == 0)
                    SessionPowerStartTimestamp = timestamp;
                LapPowerTot += power;
                if (LapPowerStartTimestamp == 0)
                    LapPowerStartTimestamp = timestamp;
            }
            else
            {
                if (SessionPowerStartTimestamp != 0)
                {
                    SessionPowerTotSeconds += lastTimestamp - SessionPowerStartTimestamp;
                    SessionPowerStartTimestamp = 0;
                }
                if (LapPowerStartTimestamp != 0)
                {
                    LapPowerTotSeconds += lastTimestamp - LapPowerStartTimestamp;
                    LapPowerStartTimestamp = 0;
                }
            }
            if (heart_rate > 0 && !LapEnd && !SessionEnd)
            {
                if (heart_rate > max_heart_rate) max_heart_rate = heart_rate;
                SessionHeart_rateTot += heart_rate;
                if (SessionHeart_rateStartTimestamp == 0)
                    SessionHeart_rateStartTimestamp = timestamp;
                LapHeart_rateTot += heart_rate;
                if (SessionHeart_rateStartTimestamp == 0)
                    SessionHeart_rateStartTimestamp = timestamp;
            }
            else
            {
                if (SessionHeart_rateStartTimestamp != 0)
                {
                    SessionHeart_rateTotSeconds += lastTimestamp - SessionHeart_rateStartTimestamp;
                    SessionHeart_rateStartTimestamp = 0;
                }
                if (LapHeart_rateStartTimestamp != 0)
                {
                    LapHeart_rateTotSeconds += lastTimestamp - LapHeart_rateStartTimestamp;
                    LapHeart_rateStartTimestamp = 0;
                }
            }
            if (cadence > 0 && !LapEnd && !SessionEnd)
            {
                if (cadence > max_cadence) max_cadence = cadence;
                SessionCadenceTot += cadence;
                if (SessionCadenceStartTimestamp == 0)
                    SessionCadenceStartTimestamp = timestamp;
                LapCadenceTot += cadence;
                if (LapCadenceStartTimestamp == 0)
                    LapCadenceStartTimestamp = timestamp;
            }
            else
            {
                if (SessionCadenceStartTimestamp != 0)
                {
                    SessionCadenceTotSeconds += lastTimestamp - SessionCadenceStartTimestamp;
                    SessionCadenceStartTimestamp = 0;
                }
                if (SessionCadenceStartTimestamp != 0)
                {
                    SessionCadenceTotSeconds += lastTimestamp - SessionCadenceStartTimestamp;
                    SessionCadenceStartTimestamp = 0;
                }
            }
            lastAltitude = altitude;
            lastTravelCM = distance;
            lastTimestamp = timestamp;
        }
    }
    public class fitFileWrite
    {
        const byte HEADERSIZE = 12;
        localMsgDef[] localMesgDefs = new localMsgDef[16];
        FitFieldStream fitstream;
        statistics st = new statistics();
        public fitFileWrite(StreamReader file, FileStream fitFile)
        {

            MemoryStream memStream = new MemoryStream();
            fitstream = new FitFieldStream(memStream);
            memStream.Seek(HEADERSIZE, SeekOrigin.Begin);
            while (!file.EndOfStream)
            {
                string[] str = file.ReadLine().Split(';');
                if (str[0] == "data")
                    writeData(str, st);
                if (str[0] == "def")
                    writeDefintion(str);
            }
            int size = (int)memStream.Position - HEADERSIZE;
            memStream.Seek(0, SeekOrigin.Begin);
            writeFitHeader(size, HEADERSIZE);
            UInt16 crc = 0;
            memStream.Seek(0, SeekOrigin.Begin);
            while (memStream.Position < memStream.Length)
            {
                crc = fitstream.Get16(crc, (byte)memStream.ReadByte());
            }
            memStream.WriteByte((byte)(crc & 0xFF));
            memStream.WriteByte((byte)(crc >> 8));
            fitFile.Write(memStream.GetBuffer(), 0, (int)memStream.Position);
        }
        private void writeData(string[] str, statistics st)
        {

            byte header = byte.Parse(str[1]);
            fitstream.writeValue(header);
            byte fields = localMesgDefs[header].Fields;
            localField[] fd = localMesgDefs[header].localFields;
            int globalMesgIndex = localMesgDefs[header].globalMesgIndex;
            if (globalMesgIndex == FIT.FIT_MESG_NUM_SESSION) st.calcSession();
            if (globalMesgIndex == FIT.FIT_MESG_NUM_LAP) st.calcLap();
            for (int i = 0; i < fields; i++)
            {
                fitstream.writeValue(fd[i], str[i + 2], st, globalMesgIndex);
            }
            if (globalMesgIndex == FIT.FIT_MESG_NUM_RECORD) st.update();
            if (globalMesgIndex == FIT.FIT_MESG_NUM_SESSION) st.resetSession();
            if (globalMesgIndex == FIT.FIT_MESG_NUM_LAP) st.resetLap();
        }
        private void writeFitHeader(Int32 size, byte headerSize)
        {
            fitstream.writeValue((byte)headerSize); //size header
            fitstream.writeValue((byte)0x10); //protocol version
            fitstream.writeValue((byte)0x40); //protocol version
            fitstream.writeValue((byte)0x00); //protocol version
            fitstream.writeValue(size, 0); //protocol version
            fitstream.writeValue('.'); //.FIT
            fitstream.writeValue('F'); //
            fitstream.writeValue('I'); //
            fitstream.writeValue('T'); //
        }
        private void writeDefintion(string[] str)
        {

            byte header = byte.Parse(str[1]);
            int localMesgIndex = header & 0x0f;
            byte reserved = byte.Parse(str[2]);
            localMsgDef msgDef = new localMsgDef();
            byte arch = msgDef.Architecture = byte.Parse(str[3]);
            UInt16 globalMesgIndex = msgDef.globalMesgIndex = UInt16.Parse(str[4]);
            byte fields = msgDef.Fields = byte.Parse(str[5]);
            msgDef.localFields = new localField[fields];
            msgDef.mesg = FIT.getMessageStruct(msgDef.globalMesgIndex);
            localMesgDefs[localMesgIndex] = msgDef;
            fitstream.writeValue(header);
            fitstream.writeValue(reserved);
            fitstream.writeValue(arch);
            fitstream.writeValue(globalMesgIndex, 0);
            fitstream.writeValue(fields);

            for (int i = 0; i < fields; i++)
            {
                localField fielddef = new localField();
                byte FieldDefinitionNumber = fielddef.FieldDefinitionNumber = byte.Parse(str[6 + (i) * 3]);
                byte size = fielddef.size = byte.Parse(str[7 + (i) * 3]);
                byte baseType = fielddef.baseType = byte.Parse(str[8 + (i) * 3]);
                fielddef.GlobalField = FIT.getFieldStruct(msgDef.mesg, (int)FieldDefinitionNumber);
                localMesgDefs[localMesgIndex].localFields[i] = fielddef;
                fitstream.writeValue(FieldDefinitionNumber);
                fitstream.writeValue(size);
                fitstream.writeValue(baseType);
            }
        }
        public class FitFieldStream
        {
            CultureInfo cuNL = FIT.cuNL;
            DateTime dt1989 = FIT.dt1989;
            MemoryStream file;

            public bool eof { get { return (file.Position + 2 > file.Length); } }
            public FitFieldStream(MemoryStream file)
            {
                this.file = file;
            }
            public void writeValue(localField localFieldDef, string value, statistics st, int globalMesgIndex)
            {
                UInt32 _UInt32 = 0;
                Int32 _Int32 = 0;
                UInt16 _UInt16 = 0;
                Int16 _Int16 = 0;
                byte _byte = 0;
                int size = localFieldDef.size;
                int arch = localFieldDef.arch;
                int baseType = localFieldDef.baseType;
                globalField globalField = localFieldDef.GlobalField;
                string fieldName = globalField.name;
                double scale = globalField.scale;
                double offset = globalField.offset;
                if (localFieldDef.FieldDefinitionNumber == 253)
                {
                    DateTime dt;
                    DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", cuNL, DateTimeStyles.None, out dt);
                    value = ((UInt32)dt.ToUniversalTime().Subtract(dt1989).Duration().TotalSeconds).ToString("0");
                }
                if (globalMesgIndex == FIT.FIT_MESG_NUM_SESSION)
                {
                    switch (fieldName)
                    {
                        case "start_time": writeValue(st.SessionStartTimestamp, arch); return;
                        case "start_position_lat": writeValue(st.start_position_lat, arch); return;
                        case "start_position_long": writeValue(st.start_position_long, arch); return;
                        case "total_elapsed_time": writeValue(st.total_elapsed_time, arch); return;
                        case "total_timer_time": writeValue(st.total_timer_time, arch); return;
                        case "total_distance": writeValue(st.total_distance, arch); return;
                        case "total_calories": writeValue(st.total_calories, arch); return;
                        case "avg_speed": writeValue(st.avg_speed, arch); return;
                        case "max_speed": writeValue(st.max_speed, arch); return;
                        case "avg_heart_rate": writeValue(st.avg_heart_rate); return;
                        case "max_heart_rate": writeValue(st.max_heart_rate); return;
                        case "avg_cadence": writeValue(st.avg_cadence); return;
                        case "max_cadence": writeValue(st.max_cadence); return;
                        case "avg_power": writeValue(st.avg_power, arch); return;
                        case "max_power": writeValue(st.max_power, arch); return;
                        case "total_ascent": writeValue((UInt16)(st.SessionTotal_ascent / 5.0), arch); return;
                        case "total_descent": writeValue((UInt16)(st.SessionTotal_descent / 5.0), arch); return;
                    }
                }
                if (globalMesgIndex == FIT.FIT_MESG_NUM_LAP)
                {
                    switch (fieldName)
                    {
                        case "start_time": writeValue(st.LapStartTimestamp, arch); return;
                        case "start_position_lat": writeValue(st.start_position_lat, arch); return;
                        case "start_position_long": writeValue(st.start_position_long, arch); return;
                        case "total_elapsed_time": writeValue(st.total_elapsed_time, arch); return;
                        case "total_timer_time": writeValue(st.total_timer_time, arch); return;
                        case "total_distance": writeValue(st.total_distance, arch); return;
                        case "total_calories": writeValue(st.total_calories, arch); return;
                        case "avg_speed": writeValue(st.avg_speed, arch); return;
                        case "max_speed": writeValue(st.max_speed, arch); return;
                        case "avg_heart_rate": writeValue(st.avg_heart_rate); return;
                        case "max_heart_rate": writeValue(st.max_heart_rate); return;
                        case "avg_cadence": writeValue(st.avg_cadence); return;
                        case "max_cadence": writeValue(st.max_cadence); return;
                        case "avg_power": writeValue(st.avg_power, arch); return;
                        case "max_power": writeValue(st.max_power, arch); return;
                        case "total_ascent": writeValue((UInt16)(st.LapTotal_ascent / 5.0), arch); return;
                        case "total_descent": writeValue((UInt16)(st.LapTotal_descent / 5.0), arch); return;
                    }
                }

                switch (baseType)
                {
                    case 0x00: writeValue(FIT.StringToEnum(fieldName, value)); break;//enum
                    case 0x01: if (value == "") writeValue((sbyte)0x7F); else writeValue((sbyte)((double.Parse(value) + offset) * scale + 0.1)); break;//sbyte
                    case 0x02: if (value == "") writeValue((byte)0xFF); else writeValue(_byte = (byte)((double.Parse(value) + offset) * scale + 0.1)); break;//byte
                    case 0x83: if (value == "") writeValue((Int16)0x7FFF, arch); else writeValue(_Int16 = (Int16)((double.Parse(value) + offset) * scale + 0.1), arch); break;//int16
                    case 0x84: if (value == "") writeValue((UInt16)0xFFFF, arch); else writeValue(_UInt16 = (UInt16)((double.Parse(value) + offset) * scale + 0.1), arch); break;//Uint16
                    case 0x85: if (value == "") writeValue((Int32)0x7FFFFFFF, arch); else writeValue(_Int32 = (Int32)((double.Parse(value) + offset) * scale + 0.1), arch); break;//int32
                    case 0x86: if (value == "") writeValue((UInt32)0xFFFFFFFF, arch); else writeValue(_UInt32 = (UInt32)((double.Parse(value) + offset) * scale + 0.1), arch); break;//Uint32
                    case 0x87: if (value == "") writeValue((UInt32)0xFFFFFFFF, arch); else writeValue((UInt32)((double.Parse(value) + offset) * scale + 0.1), arch); break;//float32
                    case 0x89: if (value == "") writeValue((UInt64)0xFFFFFFFFFFFFFFFF, arch); else writeValue((UInt64)((double.Parse(value) + offset) * scale + 0.1), arch); break;//float64
                    case 0x0A: if (value == "") writeValue((byte)0x00); else writeValue((byte)((double.Parse(value) + offset) * scale + 0.1)); break;//byteZ
                    case 0x8B: if (value == "") writeValue((UInt16)0x00, arch); else writeValue((UInt16)((double.Parse(value) + offset) * scale + 0.1), arch); break;//Uint16Z
                    case 0x8C: if (value == "") writeValue((UInt32)0x00, arch); else writeValue((UInt32)((double.Parse(value) + offset) * scale + 0.1), arch); break;//Uint32Z
                    case 0x07:
                        for (int i = 0; i < size; i++)
                            writeValue(value.ToArray()[i]);
                        break; ; //string
                    case 0x0D:
                        for (int i = 0; i < size; i++)
                            writeValue(byte.Parse(value.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier));
                        break; ; //string
                    default:
                        break;
                }
                if (globalMesgIndex == FIT.FIT_MESG_NUM_RECORD)
                    switch (fieldName)
                    {
                        case "timestamp": st.timestamp = _UInt32; break;
                        case "position_lat": if (st.start_position_lat == 0) st.start_position_lat = _Int32; break;
                        case "position_long": st.start_position_long = _Int32; break;
                        case "altitude": st.altitude = _UInt16; break;
                        case "distance": st.distance = _UInt32; break;
                        case "speed": st.speed = _UInt16; break;
                        case "power": st.power = _UInt16; break;
                        case "cadence": st.cadence = _byte; break;
                        case "heart_rate": st.heart_rate = _byte; break;
                    }
            }
            public void writeValue(char value)
            {
                file.WriteByte((byte)value);
            }
            public void writeValue(byte value)
            {
                file.WriteByte(value);
            }
            private void writeValue(sbyte value)
            {
                file.WriteByte((byte)value);
            }
            public void writeValue(Int16 value, int arch)
            {
                if (arch == 0)
                {
                    file.WriteByte((byte)value);
                    file.WriteByte((byte)(value >> 8));
                }
                else
                {
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)value);
                }
            }
            public void writeValue(UInt16 value, int arch)
            {
                if (arch == 0)
                {
                    file.WriteByte((byte)value);
                    file.WriteByte((byte)(value >> 8));
                }
                else
                {
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)value);
                }
            }
            public void writeValue(Int32 value, int arch)
            {
                if (arch == 0)
                {
                    file.WriteByte((byte)value);
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)(value >> 16));
                    file.WriteByte((byte)(value >> 24));
                }
                else
                {
                    file.WriteByte((byte)(value >> 24));
                    file.WriteByte((byte)(value >> 16));
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)value);
                }
            }
            private void writeValue(UInt32 value, int arch)
            {
                if (arch == 0)
                {
                    file.WriteByte((byte)value);
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)(value >> 16));
                    file.WriteByte((byte)(value >> 24));
                }
                else
                {
                    file.WriteByte((byte)(value >> 24));
                    file.WriteByte((byte)(value >> 16));
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)value);
                }
            }
            private void writeValue(UInt64 value, int arch)
            {
                if (arch == 0)
                {
                    file.WriteByte((byte)value);
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)(value >> 16));
                    file.WriteByte((byte)(value >> 24));
                    file.WriteByte((byte)(value >> 32));
                    file.WriteByte((byte)(value >> 48));
                    file.WriteByte((byte)(value >> 56));
                    file.WriteByte((byte)(value >> 64));
                }
                else
                {
                    file.WriteByte((byte)(value >> 64));
                    file.WriteByte((byte)(value >> 56));
                    file.WriteByte((byte)(value >> 48));
                    file.WriteByte((byte)(value >> 32));
                    file.WriteByte((byte)(value >> 24));
                    file.WriteByte((byte)(value >> 16));
                    file.WriteByte((byte)(value >> 8));
                    file.WriteByte((byte)value);
                }
            }
            unsafe public UInt16 Get16(UInt16 crc, byte b)
            {
                UInt16[] crc_table = { 0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401, 0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400 };
                UInt16 tmp;

                // compute checksum of lower four bits of byte 
                tmp = crc_table[crc & 0xF];
                crc = (UInt16)((crc >> 4) & 0x0FFF);
                crc = (UInt16)(crc ^ tmp ^ crc_table[b & 0xF]);

                // now compute checksum of upper four bits of byte 
                tmp = crc_table[crc & 0xF];
                crc = (UInt16)((crc >> 4) & 0x0FFF);
                crc = (UInt16)(crc ^ tmp ^ crc_table[(b >> 4) & 0xF]);

                return crc;
            }
        }
    }
}
