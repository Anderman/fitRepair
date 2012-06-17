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
        DateTime dt;
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
        public UInt16 total_ascent;
        public UInt16 total_descent;
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
        UInt32 startTimestamp = 0;
        UInt32 TraveltotalSeconds = 0;
        UInt32 TravelStartTimestamp = 0;
        UInt32 TravelEndTimestamp = 0;
        UInt32 TraveltotalCentimeters = 0;
        UInt32 TravelStartMeters = 0;
        UInt32 TravelEndMeters = 0;
        UInt32 totPower = 0;
        UInt32 PowertotalSeconds = 0;
        UInt32 PowerStartTimestamp = 0;
        UInt32 PowerEndTimestamp = 0;
        UInt32 totheart_rate = 0;
        UInt32 heart_ratetotalSeconds = 0;
        UInt32 heart_rateStartTimestamp = 0;
        UInt32 heart_rateEndTimestamp = 0;
        UInt32 totcadence = 0;
        UInt32 cadencetotalSeconds = 0;
        UInt32 cadenceStartTimestamp = 0;
        UInt32 cadenceEndTimestamp = 0;
        public void calcSession()
        {
            total_elapsed_time = (UInt32)((timestamp - startTimestamp) * 1000);
            total_timer_time = (UInt32)(TraveltotalSeconds * 1000);
            total_distance = distance;
            total_calories = (UInt16)(totPower / 1000.0 * 1.1 + 0.1);
            avg_speed = (UInt16)((TraveltotalCentimeters * 10.0) / (TraveltotalSeconds));
            avg_power = PowertotalSeconds == 0 ? (UInt16)0xFFFF : (UInt16)(totPower / PowertotalSeconds);
            avg_heart_rate = heart_ratetotalSeconds == 0 ? (byte)0xFF : (byte)(totheart_rate / heart_ratetotalSeconds);
            avg_cadence = cadencetotalSeconds == 0 ? (byte)0xFF : (byte)(totcadence / cadencetotalSeconds);
        }
        public void update()
        {
            if (lastAltitude == 0xFFFF) lastAltitude = altitude;
            if (altitude - lastAltitude > 0) total_ascent += (UInt16)(altitude - lastAltitude);
            if (lastAltitude - altitude > 0) total_descent += (UInt16)(lastAltitude - altitude);
            lastAltitude = altitude;

            if (startTimestamp == 0) startTimestamp = timestamp;
            if (speed > 2000)
            {
                if (speed > max_speed) max_speed = speed;
                TravelEndTimestamp = timestamp;
                TravelEndMeters = distance;
                if (TravelStartMeters == 0)
                {
                    TravelStartTimestamp = TravelEndTimestamp;
                    TravelStartMeters = TravelEndMeters;
                }
            }
            else
            {
                TraveltotalSeconds += TravelEndTimestamp - TravelStartTimestamp;
                TraveltotalCentimeters += TravelEndMeters - TravelStartMeters;
                TravelStartTimestamp = TravelEndTimestamp = 0;
                TravelStartMeters = TravelEndMeters = 0;
            }
            if (power > 0)
            {
                if (power > max_power) max_power = power;
                PowerEndTimestamp = timestamp;
                totPower += power;
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
            if (heart_rate > 0)
            {
                if (heart_rate > max_heart_rate) max_heart_rate = heart_rate;
                heart_rateEndTimestamp = timestamp;
                totheart_rate += heart_rate;
                if (heart_rateStartTimestamp == 0)
                {
                    heart_rateStartTimestamp = heart_rateEndTimestamp;
                }
            }
            else
            {
                heart_ratetotalSeconds += heart_rateEndTimestamp - heart_rateStartTimestamp;
                heart_rateStartTimestamp = heart_rateEndTimestamp = 0;
            }
            if (cadence > 0)
            {
                if (cadence > max_cadence) max_cadence = cadence;
                cadenceEndTimestamp = timestamp;
                totcadence += cadence;
                if (cadenceStartTimestamp == 0)
                {
                    cadenceStartTimestamp = cadenceEndTimestamp;
                }
            }
            else
            {
                cadencetotalSeconds += cadenceEndTimestamp - cadenceStartTimestamp;
                cadenceStartTimestamp = cadenceEndTimestamp = 0;
            }
        }
    }
    public class fitFileWrite
    {
        localMsgDef[] localMesgDefs = new localMsgDef[16];
        FitFieldStream fitstream;
        statistics st = new statistics();
        public fitFileWrite(StreamReader file, FileStream fitFile)
        {

            MemoryStream memStream = new MemoryStream();
            fitstream = new FitFieldStream(memStream);
            memStream.Seek(12, SeekOrigin.Begin);
            while (!file.EndOfStream)
            {
                string[] str = file.ReadLine().Split(';');
                if (str[0] == "data")
                    writeData(str, st);
                if (str[0] == "def")
                    writeDefintion(str);
            }
            int size = (int)memStream.Position - 12;
            memStream.Seek(0, SeekOrigin.Begin);
            writeFitHeader(size);
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
            for (int i = 0; i < fields; i++)
            {
                fitstream.writeValue(fd[i], str[i + 2], st, globalMesgIndex);
            }
            st.update();
        }
        private void writeFitHeader(Int32 size)
        {
            fitstream.writeValue((byte)0x0C); //size header
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
                    st.calcSession();
                    switch (fieldName)
                    {
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
                        case "total_ascent": writeValue((UInt16)(st.total_ascent / 5.0), arch); return;
                        case "total_descent": writeValue((UInt16)(st.total_descent / 5.0), arch); return;
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
