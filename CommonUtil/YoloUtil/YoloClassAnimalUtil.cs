namespace CommonUtil.YoloUtil;

public static class YoloClassAnimalUtil
{
    public static string GetAnimalName(string animalNo)
    {
        Dictionary<string, string> animal = new Dictionary<string, string>()
        {
            { "bmjwh", "白头卷尾猴" },
            { "bth", "白秃猴" },
            { "banbao", "斑豹" },
            { "banma", "斑马" },
            { "cw", "刺猬" },
            { "dx", "大象" },
            { "dxx", "大猩猩" },
            { "daishu", "袋鼠" },
            { "daixion", "袋熊" },
            { "ee", "鹅" },
            { "fn", "蜂鸟" },
            { "gz", "鸽子" },
            { "hz", "豪猪" },
            { "hema", "河马" },
            { "htyh", "黑头夜猴" },
            { "hwyh", "黑乌叶猴" },
            { "hjcwh", "红卷尾赤猴" },
            { "hmxx", "红毛猩猩" },
            { "huli", "狐狸" },
            { "wanxion", "浣熊" },
            { "huoji", "火鸡" },
            { "hln", "火烈鸟" },
            { "kaola", "考拉" },
            { "lan", "狼" },
            { "laohu", "老虎" },
            { "laoyin", "老鹰" },
            { "linyang", "羚羊" },
            { "lu", "鹿" },
            { "lv", "驴" },
            { "mq", "麻雀" },
            { "ma", "马" },
            { "mty", "猫头鹰" },
            { "mianyang", "绵羊" },
            { "sanyang", "山羊" },
            { "sz", "狮子" },
            { "sl", "水獭" },
            { "ss", "松鼠" },
            { "ssh", "松鼠猴" },
            { "jxm", "鹈形目" },
            { "tianer", "天鹅" },
            { "tulan", "土狼" },
            { "xiniao", "犀鸟" },
            { "xiniu", "犀牛" },
            { "xionmao", "熊猫" },
            { "xuehou", "雪猴" },
            { "yz", "鸭子" },
            { "yze", "扬子鳄" },
            { "yinro", "银狨" },
            { "que", "鹬" },
            { "zlr", "侏儒狨" },
            { "zhu", "猪" },
            { "zmn", "啄木鸟" },
            { "zmhh", "鬃毛吼猴" }
        };
        if (animal.TryGetValue(animalNo, out string value))
        {
            return value;
        }

        return "不存在匹配数据";
    }
}