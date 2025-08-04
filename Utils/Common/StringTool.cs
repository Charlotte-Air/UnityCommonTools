using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Framework.Utils
{
    /// <summary>
    /// StringTool字符串相关操作 封装一些常用操作
    /// </summary>
    public class StringTool
    {
        public static void ReadArray(ref float v, string[] arr, uint index, float default_value = 0)
        {
            v = ToFloat(arr, index, default_value);
        }

        public static void ReadArray(ref double v, string[] arr, uint index, double default_value = 0)
        {
            v = ToDouble(arr, index, default_value);
        }

        public static void ReadArray(ref int v, string[] arr, uint index, int default_value = 0)
        {
            v = ToInt(arr, index, default_value);
        }

        public static void ReadArray(ref uint v, string[] arr, uint index, uint default_value = 0)
        {
            v = ToUInt(arr, index, default_value);
        }

        public static void ReadArray(ref short v, string[] arr, uint index, short default_value = 0)
        {
            v = (short)ToInt(arr, index, default_value);
        }

        public static void ReadArray(ref ushort v, string[] arr, uint index, ushort default_value = 0)
        {
            v = (ushort)ToUInt(arr, index, default_value);
        }

        public static void ReadArray(ref bool v, string[] arr, uint index, bool default_value = false)
        {
            v = ToBool(arr, index, default_value);
        }

        public static void ReadArray(ref string v, string[] arr, uint index)
        {
            v = ToString(arr, index);
        }

        public static void ReadArray(ref string v, string[] arr, uint index, string default_value)
        {
            v = ToString(arr, index, default_value);
        }

        public static float ToFloat(string[] arr, uint index, float default_value = 0)
        {
            return arr.Length > index ? ToFloat(arr[index], default_value) : default_value;
        }

        public static double ToDouble(string[] arr, uint index, double default_value = 0)
        {
            return arr.Length > index ? ToDouble(arr[index], default_value) : default_value;
        }

        public static int ToInt(string[] arr, uint index, int default_value = 0)
        {
            return arr.Length > index ? ToInt(arr[index], default_value) : default_value;
        }

        public static uint ToUInt(string[] arr, uint index, uint default_value = 0)
        {
            return arr.Length > index ? ToUInt(arr[index], default_value) : default_value;
        }

        public static long ToLong(string[] arr, uint index, long default_value = 0)
        {
            return arr.Length > index ? ToLong(arr[index], default_value) : default_value;
        }

        public static ulong ToULong(string[] arr, uint index, ulong default_value = 0)
        {
            return arr.Length > index ? ToULong(arr[index], default_value) : default_value;
        }

        public static bool ToBool(string[] arr, uint index, bool default_value = false)
        {
            return arr.Length > index ? ToBool(arr[index], default_value) : default_value;
        }

        public static int[] ConvertInt(string[] arr)
        {
            int[] o = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                o[i] = ToInt(arr[i]);
            }

            return o;
        }

        public static uint[] ConvertUInt(string[] arr)
        {
            uint[] o = new uint[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                o[i] = ToUInt(arr[i]);
            }

            return o;
        }

        public static float[] ConvertFloat(string[] arr)
        {
            float[] o = new float[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                o[i] = ToFloat(arr[i]);
            }

            return o;
        }

        public static bool[] ConvertBool(string[] arr)
        {
            bool[] o = new bool[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                o[i] = ToBool(arr[i]);
            }

            return o;
        }

        public static string ToString(string[] arr, uint index)
        {
            return arr.Length > index ? (arr[index]) : string.Empty;
        }

        public static string ToString(string[] arr, uint index, string def)
        {
            return arr.Length > index ? (arr[index]) : def;
        }

        public static bool IsNumber(string[] arr, uint index)
        {
            if (arr.Length > index)
            {
                Double n;
                if (System.Double.TryParse(arr[index], out n))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNumber(String text)
        {
            Double n;
            if (System.Double.TryParse(text, out n))
            {
                return true;
            }

            return false;
        }

        public static bool IsNumber(String text, out Double d)
        {
            if (System.Double.TryParse(text, out d))
            {
                return true;
            }

            return false;
        }

        public static bool IsInt(String text)
        {
            int n;
            if (System.Int32.TryParse(text, out n))
            {
                return true;
            }

            return false;
        }

        public static string ToString(Vector2 pos)
        {
            return string.Format("{0},{1}", pos.x, pos.y);
        }

        // public static Color32 ToColor32(String hexTxt) {
        //     return ColorTool.HexToColor32(hexTxt);
        // }

        // public static Color32 ToColor32(string[] arr, uint index, Color32 def) {
        //     return arr.Length > index ? ToColor32(arr[index]) : def;
        // }

        // public static Color ToColor(String hexTxt) {
        //     return ColorTool.HexToColor(hexTxt);
        // }

        // public static Color ToColor(string[] arr, uint index, Color def) {
        //     return arr.Length > index ? ToColor(arr[index]) : def;
        // }

        public static Vector2 ToVector2(String text)
        {
            return ToVector2(text, Vector2.zero);
        }

        public static Vector2 ToVector2(String text, Vector2 defaultValue)
        {
            string[] array = text.Split(',');
            return new Vector2(ToFloat(array, 0, defaultValue.x), ToFloat(array, 1, defaultValue.y));
        }

        public static Vector2 ToVector2(string[] arr, uint x_index, uint y_index)
        {
            return new Vector2(ToFloat(arr, x_index), ToFloat(arr, y_index));
        }

        public static Vector2 ToVector2(string[] arr, uint x_index, uint y_index, Vector2 def)
        {
            return new Vector2(ToFloat(arr, x_index, def.x), ToFloat(arr, y_index, def.y));
        }

        public static string ToString(Vector3 pos)
        {
            return string.Format("{0},{1},{2}", pos.x, pos.y, pos.z);
        }

        public static Vector3 ToVector3(String text)
        {
            return ToVector3(text, Vector3.zero);
        }

        public static Vector3 ToVector3(String text, Vector3 defaultValue)
        {
            string[] array = text.Split(',');
            return new Vector3(ToFloat(array, 0, defaultValue.x), ToFloat(array, 1, defaultValue.y),
                ToFloat(array, 2, defaultValue.z));
        }

        public static Vector3 ToVector3(string[] arr, uint x_index, uint y_index, uint z_index)
        {
            return new Vector3(ToFloat(arr, x_index), ToFloat(arr, y_index), ToFloat(arr, z_index));
        }

        public static Vector3 ToVector3(string[] arr, uint x_index, uint y_index, uint z_index, Vector3 def)
        {
            return new Vector3(ToFloat(arr, x_index, def.x), ToFloat(arr, y_index, def.y),
                ToFloat(arr, z_index, def.z));
        }

        // public static Point ToPoint(String text) {
        //     string[] array = text.Split(',');
        //     return new Point(ToInt(array[0]), ToInt(array, 1));
        // }

        public static string ToString(UnityEngine.Rect rt)
        {
            return string.Format("{0},{1},{2},{3}", rt.xMin, rt.yMin, rt.width, rt.height);
        }

        public static Rect ToRect(String text)
        {
            string[] array = text.Split(',');
            return new Rect(ToFloat(array[0]), ToFloat(array, 1), ToFloat(array, 2), ToFloat(array, 3));
        }

        public static float ToFloat(String text, float def = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return (float)value;
            }

            return def;
            //return (float)System.Double.Parse(text);  //  会产生异常
        }

        // 解析从文件中读取的行,会自动截断行末的"\r"
        public static string[] ParseLine(String text)
        {
            string[] line = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].EndsWith("\r"))
                {
                    line[i] = line[i].Substring(0, line[i].Length - 1);
                }
            }

            return line;
        }

        // 返回beginSeparator到endSeparator之间的字符器
        public static string Find(String src, string beginSeparator, string endSeparator)
        {
            int start = src.IndexOf(beginSeparator);
            if (start < 0)
            {
                return string.Empty;
            }

            start += beginSeparator.Length;
            int end = src.IndexOf(endSeparator, start);
            if (end < 0)
            {
                return string.Empty;
            }

            return src.Substring(start, end - start);
        }

        public static double ToDouble(String text, double def = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return value;
            }

            return def;
        }

        public static long ToLong(String text, long def = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return (long)value;
            }

            return def;
        }

        public static ulong ToULong(String text, ulong def = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return (ulong)value;
            }

            return def;
        }

        public static int ToInt(String text, int def = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return (int)value;
            }

            return def;

            //int value = 0;
            //System.Int32.TryParse(text, out value);
            //return value;
        }

        public static char ToChar(String text, char def = '\0')
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            return text[0];
        }

        public static bool ToBool(String text, bool def = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return value != 0;
            }

            return def;
        }

        public static uint ToUInt(String text, uint def = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return def;
            }

            double value;
            if (System.Double.TryParse(text, out value))
            {
                return (uint)value;
            }

            return def;

            //uint value = 0;
            //System.UInt32.TryParse(text, out value);
            //return value;
        }

        public static string FormatToText(string format, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(format, args);
                }
                catch (Exception e)
                {
                    return string.Format("字符串[{0}]格式化异常:{1}", format, e);
                }
            }
            else
            {
                return format;
            }
        }

        static Encoding _byteEncoding = Encoding.Default; // Android仅支持Ascii,Unicode

        public static Encoding byteEncoding
        {
            get { return _byteEncoding; }
            set
            {
                if (value != null) _byteEncoding = value;
                else Debug.LogError("不能设解码为空对象");
            }
        }

        public static string ToString(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                return "";
            }

            return _byteEncoding.GetString(inputBytes);
        }

        public static byte[] ToByte(String inputString)
        {
            return _byteEncoding.GetBytes(inputString);
        }

        public static uint[] ToArrayUInt(String str, char splitChar = ',', bool emptyZeroArray = false)
        {
            if (string.IsNullOrEmpty(str) && emptyZeroArray)
            {
                return new uint[0];
            }

            string[] arr = str.Split(splitChar);
            uint[] rv = new uint[arr.Length];
            for (uint i = 0; i < arr.Length; i++)
            {
                rv[i] = StringTool.ToUInt(arr, i);
            }

            return rv;
        }

        public static int[] ToArrayInt(String str, char splitChar = ',', bool emptyZeroArray = false)
        {
            if (string.IsNullOrEmpty(str) && emptyZeroArray)
            {
                return new int[0];
            }

            string[] arr = str.Split(splitChar);
            int[] rv = new int[arr.Length];
            for (uint i = 0; i < arr.Length; i++)
            {
                rv[i] = StringTool.ToInt(arr, i);
            }

            return rv;
        }

        public static float[] ToArrayFloat(String str, char splitChar = ',', bool emptyZeroArray = false)
        {
            if (string.IsNullOrEmpty(str) && emptyZeroArray)
            {
                return new float[0];
            }

            string[] arr = str.Split(splitChar);
            float[] rv = new float[arr.Length];
            for (uint i = 0; i < arr.Length; i++)
            {
                rv[i] = StringTool.ToFloat(arr, i);
            }

            return rv;
        }

        public static double[] ToArrayDouble(String str, char splitChar = ',', bool emptyZeroArray = false)
        {
            if (string.IsNullOrEmpty(str) && emptyZeroArray)
            {
                return new double[0];
            }

            string[] arr = str.Split(splitChar);
            double[] rv = new double[arr.Length];
            for (uint i = 0; i < arr.Length; i++)
            {
                rv[i] = StringTool.ToDouble(arr, i);
            }

            return rv;
        }

        // 保留分隔串，去除空行
        public static string[] Split(String text, params string[] split)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new string [0];
            }

            if (split.Length == 0)
            {
                return new string [1] { text };
            }

            int count = split.Length;
            List<String> lst = new List<string>();
            lst.Capacity = 16;
            int[] pos = new int[count];
            for (int i = 0; i < count; i++)
            {
                pos[i] = text.IndexOf(split[i]);
            }

            int length = text.Length;
            int minPos;
            int index;
            int strIndex = 0;
            do
            {
                // 寻找最小的索引
                minPos = length;
                index = -1;
                for (int i = 0; i < count; i++)
                {
                    if (pos[i] >= 0)
                    {
                        if (minPos > pos[i])
                        {
                            minPos = pos[i];
                            index = i;
                        }
                    }
                }

                if (index == -1)
                {
                    if (length > strIndex)
                    {
                        lst.Add(text.Substring(strIndex, length - strIndex));
                    }

                    break;
                }

                if (strIndex != minPos)
                {
                    lst.Add(text.Substring(strIndex, minPos - strIndex));
                }

                pos[index] = text.IndexOf(split[index], minPos + split[index].Length);
                lst.Add(split[index]);
                strIndex = minPos + split[index].Length;
            } while (true);

            return lst.ToArray();

            //StringBuilder b = new StringBuilder(128);
            //b.Append("(");
            //b.Append(split[0]);
            //for (int i = 1; i < split.Length; i++) {
            //    b.Append("|");
            //    b.Append(split[i]);
            //}
            //b.Append(")");
            //return Regex.Split(text, b.ToString());
        }

        public static string TextAddSpace(String text, string spaceChar = " ")
        {
            if (text.Length == 0)
            {
                return text;
            }

            StringBuilder b = new StringBuilder(text.Length + spaceChar.Length * text.Length);
            b.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                b.Append(spaceChar);
                b.Append(text[i]);
            }

            return b.ToString();
        }

        public static string Merge(String[] array, string splitChar = ",", int startIndex = 0, int length = -1)
        {
            if (array == null || startIndex >= array.Length)
            {
                return string.Empty;
            }

            int maxIndex;
            if (length < 0)
            {
                maxIndex = array.Length;
            }
            else
            {
                maxIndex = Math.Min(startIndex + length, array.Length);
            }

            StringBuilder b = new StringBuilder(maxIndex * 2);
            b.Append(array[startIndex]);
            for (int i = startIndex + 1; i < maxIndex; i++)
            {
                b.Append(splitChar);
                b.Append(array[i]);
            }

            return b.ToString();
        }

        //将byte数组可视化
        public static string BytesString(byte[] array, string sep = "-", int startIndex = 0, int length = -1)
        {
            if (array == null || startIndex >= array.Length)
            {
                return string.Empty;
            }

            int maxIndex;
            if (length < 0)
            {
                maxIndex = array.Length;
            }
            else
            {
                maxIndex = Math.Min(startIndex + length, array.Length);
            }

            var b = new StringBuilder(array.Length * (2 + sep.Length));
            b.Append(array[startIndex].ToString("X2"));
            for (int i = startIndex + 1; i < maxIndex; i++)
            {
                b.Append(sep);
                b.Append(array[i].ToString("X2"));
            }

            return b.ToString();
        }

        // 获得汉字拼音首字母
        public static string GetChineseSpell(String strText)
        {
            int len = strText.Length;
            StringBuilder sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                sb.Append(getSpell(strText.Substring(i, 1)));
            }

            return sb.ToString();
        }

        static string getSpell(String cnChar)
        {
            Encoding encode = Encoding.Default;
            byte[] arrCN = encode.GetBytes(cnChar);
            byte[] b = new byte[1];
            if (arrCN.Length > 1)
            {
                int area = (short)arrCN[0];
                int pos = (short)arrCN[1];
                int code = (area << 8) + pos;
                int[] areacode =
                {
                    45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371,
                    50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481
                };
                for (int i = 0; i < 26; i++)
                {
                    int max = 55290;
                    if (i != 25) max = areacode[i + 1];
                    if (areacode[i] <= code && code < max)
                    {
                        b[0] = (byte)(65 + i);
                        return encode.GetString(b);
                    }
                }

                return "*";
            }
            else
            {
                return cnChar;
            }
        }

        // 将 GB2312 中的繁体字转换为简体字
        public static string ToSimplifyString(string TraditionalString)
        {
            StringBuilder sb = new StringBuilder(TraditionalString.Length);
            string simp =
                "锕皑蔼碍爱嗳嫒瑷暧霭谙铵鹌肮袄奥媪骜鳌坝罢钯摆败呗颁办绊钣帮绑镑谤剥饱宝报鲍鸨龅辈贝钡狈备惫鹎贲锛绷笔毕毙币闭荜哔滗铋筚跸边编贬变辩辫苄缏笾标骠飑飙镖镳鳔鳖别瘪濒滨宾摈傧缤槟殡膑镔髌鬓饼禀拨钵铂驳饽钹鹁补钸财参蚕残惭惨灿骖黪苍舱仓沧厕侧册测恻层诧锸侪钗搀掺蝉馋谗缠铲产阐颤冁谄谶蒇忏婵骣觇禅镡场尝长偿肠厂畅伥苌怅阊鲳钞车彻砗尘陈衬伧谌榇碜龀撑称惩诚骋枨柽铖铛痴迟驰耻齿炽饬鸱冲冲虫宠铳畴踌筹绸俦帱雠橱厨锄雏础储触处刍绌蹰传钏疮闯创怆锤缍纯鹑绰辍龊辞词赐鹚聪葱囱从丛苁骢枞凑辏蹿窜撺错锉鹾达哒鞑带贷骀绐担单郸掸胆惮诞弹殚赕瘅箪当挡党荡档谠砀裆捣岛祷导盗焘灯邓镫敌涤递缔籴诋谛绨觌镝颠点垫电巅钿癫钓调铫鲷谍叠鲽钉顶锭订铤丢铥东动栋冻岽鸫窦犊独读赌镀渎椟牍笃黩锻断缎簖兑队对怼镦吨顿钝炖趸夺堕铎鹅额讹恶饿谔垩阏轭锇锷鹗颚颛鳄诶儿尔饵贰迩铒鸸鲕发罚阀珐矾钒烦贩饭访纺钫鲂飞诽废费绯镄鲱纷坟奋愤粪偾丰枫锋风疯冯缝讽凤沣肤辐抚辅赋复负讣妇缚凫驸绂绋赙麸鲋鳆钆该钙盖赅杆赶秆赣尴擀绀冈刚钢纲岗戆镐睾诰缟锆搁鸽阁铬个纥镉颍给亘赓绠鲠龚宫巩贡钩沟苟构购够诟缑觏蛊顾诂毂钴锢鸪鹄鹘剐挂鸹掴关观馆惯贯诖掼鹳鳏广犷规归龟闺轨诡贵刽匦刿妫桧鲑鳜辊滚衮绲鲧锅国过埚呙帼椁蝈铪骇韩汉阚绗颉号灏颢阂鹤贺诃阖蛎横轰鸿红黉讧荭闳鲎壶护沪户浒鹕哗华画划话骅桦铧怀坏欢环还缓换唤痪焕涣奂缳锾鲩黄谎鳇挥辉毁贿秽会烩汇讳诲绘诙荟哕浍缋珲晖荤浑诨馄阍获货祸钬镬击机积饥迹讥鸡绩缉极辑级挤几蓟剂济计记际继纪讦诘荠叽哜骥玑觊齑矶羁虿跻霁鲚鲫夹荚颊贾钾价驾郏浃铗镓蛲歼监坚笺间艰缄茧检碱硷拣捡简俭减荐槛鉴践贱见键舰剑饯渐溅涧谏缣戋戬睑鹣笕鲣鞯将浆蒋桨奖讲酱绛缰胶浇骄娇搅铰矫侥脚饺缴绞轿较挢峤鹪鲛阶节洁结诫届疖颌鲒紧锦仅谨进晋烬尽劲荆茎卺荩馑缙赆觐鲸惊经颈静镜径痉竞净刭泾迳弪胫靓纠厩旧阄鸠鹫驹举据锯惧剧讵屦榉飓钜锔窭龃鹃绢锩镌隽觉决绝谲珏钧军骏皲开凯剀垲忾恺铠锴龛闶钪铐颗壳课骒缂轲钶锞颔垦恳龈铿抠库裤喾块侩郐哙脍宽狯髋矿旷况诓诳邝圹纩贶亏岿窥馈溃匮蒉愦聩篑阃锟鲲扩阔蛴蜡腊莱来赖崃徕涞濑赉睐铼癞籁蓝栏拦篮阑兰澜谰揽览懒缆烂滥岚榄斓镧褴琅阆锒捞劳涝唠崂铑铹痨乐鳓镭垒类泪诔缧篱狸离鲤礼丽厉励砾历沥隶俪郦坜苈莅蓠呖逦骊缡枥栎轹砺锂鹂疠粝跞雳鲡鳢俩联莲连镰怜涟帘敛脸链恋炼练蔹奁潋琏殓裢裣鲢粮凉两辆谅魉疗辽镣缭钌鹩猎临邻鳞凛赁蔺廪檩辚躏龄铃灵岭领绫棂蛏鲮馏刘浏骝绺镏鹨龙聋咙笼垄拢陇茏泷珑栊胧砻楼娄搂篓偻蒌喽嵝镂瘘耧蝼髅芦卢颅庐炉掳卤虏鲁赂禄录陆垆撸噜闾泸渌栌橹轳辂辘氇胪鸬鹭舻鲈峦挛孪滦乱脔娈栾鸾銮抡轮伦仑沦纶论囵萝罗逻锣箩骡骆络荦猡泺椤脶镙驴吕铝侣屡缕虑滤绿榈褛锊呒妈玛码蚂马骂吗唛嬷杩买麦卖迈脉劢瞒馒蛮满谩缦镘颡鳗猫锚铆贸麽没镁门闷们扪焖懑钔锰梦眯谜弥觅幂芈谧猕祢绵缅渑腼黾庙缈缪灭悯闽闵缗鸣铭谬谟蓦馍殁镆谋亩钼呐钠纳难挠脑恼闹铙讷馁内拟腻铌鲵撵辇鲶酿鸟茑袅聂啮镊镍陧蘖嗫颟蹑柠狞宁拧泞苎咛聍钮纽脓浓农侬哝驽钕诺傩疟欧鸥殴呕沤讴怄瓯盘蹒庞抛疱赔辔喷鹏纰罴铍骗谝骈飘缥频贫嫔苹凭评泼颇钋扑铺朴谱镤镨栖脐齐骑岂启气弃讫蕲骐绮桤碛颀颃鳍牵钎铅迁签谦钱钳潜浅谴堑佥荨悭骞缱椠钤枪呛墙蔷强抢嫱樯戗炝锖锵镪羟跄锹桥乔侨翘窍诮谯荞缲硗跷窃惬锲箧钦亲寝锓轻氢倾顷请庆揿鲭琼穷茕蛱巯赇虮鳅趋区躯驱龋诎岖阒觑鸲颧权劝诠绻辁铨却鹊确阕阙悫让饶扰绕荛娆桡热韧认纫饪轫荣绒嵘蝾缛铷颦软锐蚬闰润洒萨飒鳃赛伞毵糁丧骚扫缫涩啬铯穑杀刹纱铩鲨筛晒酾删闪陕赡缮讪姗骟钐鳝墒伤赏垧殇觞烧绍赊摄慑设厍滠畲绅审婶肾渗诜谂渖声绳胜师狮湿诗时蚀实识驶势适释饰视试谥埘莳弑轼贳铈鲥寿兽绶枢输书赎属术树竖数摅纾帅闩双谁税顺说硕烁铄丝饲厮驷缌锶鸶耸怂颂讼诵擞薮馊飕锼苏诉肃谡稣虽随绥岁谇孙损笋荪狲缩琐锁唢睃獭挞闼铊鳎台态钛鲐摊贪瘫滩坛谭谈叹昙钽锬顸汤烫傥饧铴镗涛绦讨韬铽腾誊锑题体屉缇鹈阗条粜龆鲦贴铁厅听烃铜统恸头钭秃图钍团抟颓蜕饨脱鸵驮驼椭箨鼍袜娲腽弯湾顽万纨绾网辋韦违围为潍维苇伟伪纬谓卫诿帏闱沩涠玮韪炜鲔温闻纹稳问阌瓮挝蜗涡窝卧莴龌呜钨乌诬无芜吴坞雾务误邬庑怃妩骛鹉鹜锡牺袭习铣戏细饩阋玺觋虾辖峡侠狭厦吓硖鲜纤贤衔闲显险现献县馅羡宪线苋莶藓岘猃娴鹇痫蚝籼跹厢镶乡详响项芗饷骧缃飨萧嚣销晓啸哓潇骁绡枭箫协挟携胁谐写泻谢亵撷绁缬锌衅兴陉荥凶汹锈绣馐鸺虚嘘须许叙绪续诩顼轩悬选癣绚谖铉镟学谑泶鳕勋询寻驯训讯逊埙浔鲟压鸦鸭哑亚讶垭娅桠氩阉烟盐严岩颜阎艳厌砚彦谚验厣赝俨兖谳恹闫酽魇餍鼹鸯杨扬疡阳痒养样炀瑶摇尧遥窑谣药轺鹞鳐爷页业叶靥谒邺晔烨医铱颐遗仪蚁艺亿忆义诣议谊译异绎诒呓峄饴怿驿缢轶贻钇镒镱瘗舣荫阴银饮隐铟瘾樱婴鹰应缨莹萤营荧蝇赢颖茔莺萦蓥撄嘤滢潆璎鹦瘿颏罂哟拥佣痈踊咏镛优忧邮铀犹诱莸铕鱿舆鱼渔娱与屿语狱誉预驭伛俣谀谕蓣嵛饫阈妪纡觎欤钰鹆鹬龉鸳渊辕园员圆缘远橼鸢鼋约跃钥粤悦阅钺郧匀陨运蕴酝晕韵郓芸恽愠纭韫殒氲杂灾载攒暂赞瓒趱錾赃脏驵凿枣责择则泽赜啧帻箦贼谮赠综缯轧铡闸栅诈斋债毡盏斩辗崭栈战绽谵张涨帐账胀赵诏钊蛰辙锗这谪辄鹧贞针侦诊镇阵浈缜桢轸赈祯鸩挣睁狰争帧症郑证诤峥钲铮筝织职执纸挚掷帜质滞骘栉栀轵轾贽鸷蛳絷踬踯觯钟终种肿众锺诌轴皱昼骤纣绉猪诸诛烛瞩嘱贮铸驻伫槠铢专砖转赚啭馔颞桩庄装妆壮状锥赘坠缀骓缒谆准着浊诼镯兹资渍谘缁辎赀眦锱龇鲻踪总纵偬邹诹驺鲰诅组镞钻缵躜鳟翱并卜沉丑淀迭斗范干皋硅柜后伙秸杰诀夸里凌么霉捻凄扦圣尸抬涂洼喂污锨咸蝎彝涌游吁御愿岳云灶扎札筑于志注凋讠谫郄勐凼坂垅垴埯埝苘荬荮莜莼菰藁揸吒吣咔咝咴噘噼嚯幞岙嵴彷徼犸狍馀馇馓馕愣憷懔丬溆滟溷漤潴澹甯纟绔绱珉枧桊桉槔橥轱轷赍肷胨飚煳煅熘愍淼砜磙眍钚钷铘铞锃锍锎锏锘锝锪锫锿镅镎镢镥镩镲稆鹋鹛鹱疬疴痖癯裥襁耢颥螨麴鲅鲆鲇鲞鲴鲺鲼鳊鳋鳘鳙鞒鞴齄";
            string trad =
                "錒皚藹礙愛噯嬡璦曖靄諳銨鵪骯襖奧媼驁鰲壩罷鈀擺敗唄頒辦絆鈑幫綁鎊謗剝飽寶報鮑鴇齙輩貝鋇狽備憊鵯賁錛繃筆畢斃幣閉蓽嗶潷鉍篳蹕邊編貶變辯辮芐緶籩標驃颮飆鏢鑣鰾鱉別癟瀕濱賓擯儐繽檳殯臏鑌髕鬢餅稟撥缽鉑駁餑鈸鵓補鈽財參蠶殘慚慘燦驂黲蒼艙倉滄廁側冊測惻層詫鍤儕釵攙摻蟬饞讒纏鏟產闡顫囅諂讖蕆懺嬋驏覘禪鐔場嘗長償腸廠暢倀萇悵閶鯧鈔車徹硨塵陳襯傖諶櫬磣齔撐稱懲誠騁棖檉鋮鐺癡遲馳恥齒熾飭鴟沖衝蟲寵銃疇躊籌綢儔幬讎櫥廚鋤雛礎儲觸處芻絀躕傳釧瘡闖創愴錘綞純鶉綽輟齪辭詞賜鶿聰蔥囪從叢蓯驄樅湊輳躥竄攛錯銼鹺達噠韃帶貸駘紿擔單鄲撣膽憚誕彈殫賧癉簞當擋黨蕩檔讜碭襠搗島禱導盜燾燈鄧鐙敵滌遞締糴詆諦綈覿鏑顛點墊電巔鈿癲釣調銚鯛諜疊鰈釘頂錠訂鋌丟銩東動棟凍崠鶇竇犢獨讀賭鍍瀆櫝牘篤黷鍛斷緞籪兌隊對懟鐓噸頓鈍燉躉奪墮鐸鵝額訛惡餓諤堊閼軛鋨鍔鶚顎顓鱷誒兒爾餌貳邇鉺鴯鮞發罰閥琺礬釩煩販飯訪紡鈁魴飛誹廢費緋鐨鯡紛墳奮憤糞僨豐楓鋒風瘋馮縫諷鳳灃膚輻撫輔賦復負訃婦縛鳧駙紱紼賻麩鮒鰒釓該鈣蓋賅桿趕稈贛尷搟紺岡剛鋼綱崗戇鎬睪誥縞鋯擱鴿閣鉻個紇鎘潁給亙賡綆鯁龔宮鞏貢鉤溝茍構購夠詬緱覯蠱顧詁轂鈷錮鴣鵠鶻剮掛鴰摑關觀館慣貫詿摜鸛鰥廣獷規歸龜閨軌詭貴劊匭劌媯檜鮭鱖輥滾袞緄鯀鍋國過堝咼幗槨蟈鉿駭韓漢闞絎頡號灝顥閡鶴賀訶闔蠣橫轟鴻紅黌訌葒閎鱟壺護滬戶滸鶘嘩華畫劃話驊樺鏵懷壞歡環還緩換喚瘓煥渙奐繯鍰鯇黃謊鰉揮輝毀賄穢會燴匯諱誨繪詼薈噦澮繢琿暉葷渾諢餛閽獲貨禍鈥鑊擊機積饑跡譏雞績緝極輯級擠幾薊劑濟計記際繼紀訐詰薺嘰嚌驥璣覬齏磯羈蠆躋霽鱭鯽夾莢頰賈鉀價駕郟浹鋏鎵蟯殲監堅箋間艱緘繭檢堿鹼揀撿簡儉減薦檻鑒踐賤見鍵艦劍餞漸濺澗諫縑戔戩瞼鶼筧鰹韉將漿蔣槳獎講醬絳韁膠澆驕嬌攪鉸矯僥腳餃繳絞轎較撟嶠鷦鮫階節潔結誡屆癤頜鮚緊錦僅謹進晉燼盡勁荊莖巹藎饉縉贐覲鯨驚經頸靜鏡徑痙競凈剄涇逕弳脛靚糾廄舊鬮鳩鷲駒舉據鋸懼劇詎屨櫸颶鉅鋦窶齟鵑絹錈鐫雋覺決絕譎玨鈞軍駿皸開凱剴塏愾愷鎧鍇龕閌鈧銬顆殼課騍緙軻鈳錁頷墾懇齦鏗摳庫褲嚳塊儈鄶噲膾寬獪髖礦曠況誆誑鄺壙纊貺虧巋窺饋潰匱蕢憒聵簣閫錕鯤擴闊蠐蠟臘萊來賴崍徠淶瀨賚睞錸癩籟藍欄攔籃闌蘭瀾讕攬覽懶纜爛濫嵐欖斕鑭襤瑯閬鋃撈勞澇嘮嶗銠鐒癆樂鰳鐳壘類淚誄縲籬貍離鯉禮麗厲勵礫歷瀝隸儷酈壢藶蒞蘺嚦邐驪縭櫪櫟轢礪鋰鸝癘糲躒靂鱺鱧倆聯蓮連鐮憐漣簾斂臉鏈戀煉練蘞奩瀲璉殮褳襝鰱糧涼兩輛諒魎療遼鐐繚釕鷯獵臨鄰鱗凜賃藺廩檁轔躪齡鈴靈嶺領綾欞蟶鯪餾劉瀏騮綹鎦鷚龍聾嚨籠壟攏隴蘢瀧瓏櫳朧礱樓婁摟簍僂蔞嘍嶁鏤瘺耬螻髏蘆盧顱廬爐擄鹵虜魯賂祿錄陸壚擼嚕閭瀘淥櫨櫓轤輅轆氌臚鸕鷺艫鱸巒攣孿灤亂臠孌欒鸞鑾掄輪倫侖淪綸論圇蘿羅邏鑼籮騾駱絡犖玀濼欏腡鏍驢呂鋁侶屢縷慮濾綠櫚褸鋝嘸媽瑪碼螞馬罵嗎嘜嬤榪買麥賣邁脈勱瞞饅蠻滿謾縵鏝顙鰻貓錨鉚貿麼沒鎂門悶們捫燜懣鍆錳夢瞇謎彌覓冪羋謐獼禰綿緬澠靦黽廟緲繆滅憫閩閔緡鳴銘謬謨驀饃歿鏌謀畝鉬吶鈉納難撓腦惱鬧鐃訥餒內擬膩鈮鯢攆輦鯰釀鳥蔦裊聶嚙鑷鎳隉蘗囁顢躡檸獰寧擰濘苧嚀聹鈕紐膿濃農儂噥駑釹諾儺瘧歐鷗毆嘔漚謳慪甌盤蹣龐拋皰賠轡噴鵬紕羆鈹騙諞駢飄縹頻貧嬪蘋憑評潑頗釙撲鋪樸譜鏷鐠棲臍齊騎豈啟氣棄訖蘄騏綺榿磧頎頏鰭牽釬鉛遷簽謙錢鉗潛淺譴塹僉蕁慳騫繾槧鈐槍嗆墻薔強搶嬙檣戧熗錆鏘鏹羥蹌鍬橋喬僑翹竅誚譙蕎繰磽蹺竊愜鍥篋欽親寢鋟輕氫傾頃請慶撳鯖瓊窮煢蛺巰賕蟣鰍趨區軀驅齲詘嶇闃覷鴝顴權勸詮綣輇銓卻鵲確闋闕愨讓饒擾繞蕘嬈橈熱韌認紉飪軔榮絨嶸蠑縟銣顰軟銳蜆閏潤灑薩颯鰓賽傘毿糝喪騷掃繅澀嗇銫穡殺剎紗鎩鯊篩曬釃刪閃陜贍繕訕姍騸釤鱔墑傷賞坰殤觴燒紹賒攝懾設厙灄畬紳審嬸腎滲詵諗瀋聲繩勝師獅濕詩時蝕實識駛勢適釋飾視試謚塒蒔弒軾貰鈰鰣壽獸綬樞輸書贖屬術樹豎數攄紓帥閂雙誰稅順說碩爍鑠絲飼廝駟緦鍶鷥聳慫頌訟誦擻藪餿颼鎪蘇訴肅謖穌雖隨綏歲誶孫損筍蓀猻縮瑣鎖嗩脧獺撻闥鉈鰨臺態鈦鮐攤貪癱灘壇譚談嘆曇鉭錟頇湯燙儻餳鐋鏜濤絳討韜鋱騰謄銻題體屜緹鵜闐條糶齠鰷貼鐵廳聽烴銅統慟頭鈄禿圖釷團摶頹蛻飩脫鴕馱駝橢籜鼉襪媧膃彎灣頑萬紈綰網輞韋違圍為濰維葦偉偽緯謂衛諉幃闈溈潿瑋韙煒鮪溫聞紋穩問閿甕撾蝸渦窩臥萵齷嗚鎢烏誣無蕪吳塢霧務誤鄔廡憮嫵騖鵡鶩錫犧襲習銑戲細餼鬩璽覡蝦轄峽俠狹廈嚇硤鮮纖賢銜閑顯險現獻縣餡羨憲線莧薟蘚峴獫嫻鷴癇蠔秈躚廂鑲鄉詳響項薌餉驤緗饗蕭囂銷曉嘯嘵瀟驍綃梟簫協挾攜脅諧寫瀉謝褻擷紲纈鋅釁興陘滎兇洶銹繡饈鵂虛噓須許敘緒續詡頊軒懸選癬絢諼鉉鏇學謔澩鱈勛詢尋馴訓訊遜塤潯鱘壓鴉鴨啞亞訝埡婭椏氬閹煙鹽嚴巖顏閻艷厭硯彥諺驗厴贗儼兗讞懨閆釅魘饜鼴鴦楊揚瘍陽癢養樣煬瑤搖堯遙窯謠藥軺鷂鰩爺頁業葉靨謁鄴曄燁醫銥頤遺儀蟻藝億憶義詣議誼譯異繹詒囈嶧飴懌驛縊軼貽釔鎰鐿瘞艤蔭陰銀飲隱銦癮櫻嬰鷹應纓瑩螢營熒蠅贏穎塋鶯縈鎣攖嚶瀅瀠瓔鸚癭頦罌喲擁傭癰踴詠鏞優憂郵鈾猶誘蕕銪魷輿魚漁娛與嶼語獄譽預馭傴俁諛諭蕷崳飫閾嫗紆覦歟鈺鵒鷸齬鴛淵轅園員圓緣遠櫞鳶黿約躍鑰粵悅閱鉞鄖勻隕運蘊醞暈韻鄆蕓惲慍紜韞殞氳雜災載攢暫贊瓚趲鏨贓臟駔鑿棗責擇則澤賾嘖幘簀賊譖贈綜繒軋鍘閘柵詐齋債氈盞斬輾嶄棧戰綻譫張漲帳賬脹趙詔釗蟄轍鍺這謫輒鷓貞針偵診鎮陣湞縝楨軫賑禎鴆掙睜猙爭幀癥鄭證諍崢鉦錚箏織職執紙摯擲幟質滯騭櫛梔軹輊贄鷙螄縶躓躑觶鐘終種腫眾鍾謅軸皺晝驟紂縐豬諸誅燭矚囑貯鑄駐佇櫧銖專磚轉賺囀饌顳樁莊裝妝壯狀錐贅墜綴騅縋諄準著濁諑鐲茲資漬諮緇輜貲眥錙齜鯔蹤總縱傯鄒諏騶鯫詛組鏃鉆纘躦鱒翺並蔔沈醜澱叠鬥範幹臯矽櫃後夥稭傑訣誇裏淩麽黴撚淒扡聖屍擡塗窪餵汙鍁鹹蠍彜湧遊籲禦願嶽雲竈紮劄築於誌註雕訁譾郤猛氹阪壟堖垵墊檾蕒葤蓧蒓菇槁摣咤唚哢噝噅撅劈謔襆嶴脊仿僥獁麅餘餷饊饢楞怵懍爿漵灩混濫瀦淡寧糸絝緔瑉梘棬案橰櫫軲軤賫膁腖飈糊煆溜湣渺碸滾瞘鈈鉕鋣銱鋥鋶鐦鐧鍩鍀鍃錇鎄鎇鎿鐝鑥鑹鑔穭鶓鶥鸌癧屙瘂臒襇繈耮顬蟎麯鮁鮃鮎鯗鯝鯴鱝鯿鰠鰵鱅鞽韝齇";

            char[] tradArray = TraditionalString.ToCharArray();
            for (int i = 0; i < tradArray.Length; i++)
            {
                char c = tradArray[i];
                if (trad.IndexOf(c) >= 0)
                {
                    sb.Append(simp.Substring(trad.IndexOf(c), 1));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        // 将全角字符转换为半角字符
        public static string CnChar2EnChar(string s)
        {
            string cnChars = "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９";
            string enChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            StringBuilder sb = new StringBuilder(s.Length);
            char[] cArray = s.ToCharArray();

            for (int i = 0; i < cArray.Length; i++)
            {
                char c = cArray[i];
                if (cnChars.IndexOf(c) >= 0)
                {
                    sb.Append(enChars.Substring(cnChars.IndexOf(c), 1));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        // 预处理，去掉不必要的HTML信息，并完成繁体、简体转换
        public static string HTML2Text(string content)
        {
            //去掉多余空白符号
            content = Regex.Replace(content, @"\s+", " ");

            //将换行符更换成回车换行标记
            content = Regex.Replace(content, @"<br>", "\n\r", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"</br>", "\n\r", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"<p>", "\n\r", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"</p>", "\n\r", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"<p( )*/>", "\n\r", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"<br( )*/>", "\n\r", RegexOptions.IgnoreCase);

            //去掉所有HTML标记
            content = Regex.Replace(content, @"<[^>]+?>", " ");

            //将所有代换符还原
            content = Regex.Replace(content, @"&nbsp;", " ");
            content = Regex.Replace(content, @"&gt;", ">");
            content = Regex.Replace(content, @"&lt;", "<");
            content = Regex.Replace(content, @"&amp;", "&");
            content = Regex.Replace(content, @"&quot;", "\"");

            content = Regex.Replace(content, @"\n[\s| ]*\r", "\n\r"); //去掉空行
            content = Regex.Replace(content, @"( ){2,}", " "); //将多个连续空格替换成一个空格
            content = Regex.Replace(content, @"(^\s*)|(\s*$)", ""); //去掉首尾空格
            content = Regex.Replace(content, @"\n\r( )*", "\n\r"); //去掉行首空格
            content = Regex.Replace(content, @"( )*\n\r", "\n\r"); //去掉行尾空格
            return content;
        }
    }
}