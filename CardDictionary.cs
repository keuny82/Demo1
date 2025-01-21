using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    class GameCardElement : TableElement
    {
        public int CardColor;
        public int CardNo;
        public int Attack;
        public int Defence;
        public void Serialize(TableElementSerializer helper)
        {
            helper.Bind(ref CardColor, "Color");
            helper.Bind(ref CardNo, "No");
            helper.Bind(ref Attack, "Attack");
            helper.Bind(ref Defence, "Defence");
        }
    }

    public class GameCard
    {
        public int CardColor;
        public int CardNo;
        public int Attack;
        public int Defence;
    }

    class GameCardTableLoader : ThreadSafeSingleton<GameCardTableLoader>, ITableLoader
    {
        public List<GameCard> m_GameCards;

        public GameCardTableLoader() { Load(); }
        public bool Load()
        {
            m_GameCards = new List<GameCard>();
            try
            {
                TableConverterBase<GameCardElement> table = new TableConverterBase<GameCardElement>(
                    1,
                    //System.IO.Directory.GetCurrentDirectory() + "\\table\\CardData.xml",
                    TableLoaderContainer.Instance.GetTableSrc() + "CardData.xml",
                    "Card", true);

                table.Done();

                if (!table.mydataValid)
                    return false;

                foreach (var ele in table.mydata)
                {
                    GameCard card = new GameCard();
                    card.CardNo = ele.CardNo;
                    card.CardColor = ele.CardColor;
                    card.Attack = ele.Attack;
                    card.Defence = ele.Defence;

                    m_GameCards.Add(card);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
