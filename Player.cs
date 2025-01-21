using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Demo1
{
    class Player
    {
        public string Name { get; set; }
        public List<GameCard> Deck { get; set; } = new List<GameCard>();
        public List<GameCard> Hand { get; set; } = new List<GameCard>();
        public List<GameCard> Trash { get; set; } = new List<GameCard>();
        public int HP { get; set; } = 10;        
        public bool IsMyTurn { get; set; } = false;
        public bool Ready { get; set; }
        public Socket ClientSocket { get; set; }

        public int CurrentRoomID { get; set; }

        public Player(string name, Socket socket)
        {
            Name = name;
            ClientSocket = socket;
        }

        public Socket GetSocket() { return ClientSocket; }

        public void SetCurrentRoomID(int roomID) { CurrentRoomID = roomID; }
        public int GetCurrentRoomID() { return CurrentRoomID; }
        public bool IsSetMyInfo() { return Name.Length > 0 ? true : false; }
        public void SetReady(bool ready) { Ready = ready; }
        public bool GetReady() { return Ready; }

        public void DrawCard()
        {
            Hand.Add(Deck[0]);
            Console.WriteLine($"{Name} add card : {Deck[0].CardNo}");
            Deck.RemoveAt(0);
        }

        public void AddTrash(GameCard card)
        {
            Trash.Add(card);
            Console.WriteLine($"{Name} trash card : {card.CardNo}");
        }

        public void AddCardToDeck(GameCard card)
        {
            Deck.Add(card);
            Console.WriteLine($"{Name}'s {card.CardNo} is add deck");
            Hand.Remove(card);
        }

        public void TakeDamage(int damage)
        {
            HP -= damage;
            Console.WriteLine($"{Name} take damage : {damage}, HP remaining : {HP}");
        }

        public void PlayerCard(GameCard card)
        {
            if(Hand.Contains(card))
            {
                Hand.Remove(card);
                Console.WriteLine($"{Name} Played {card}");
            }
        }

        public void ShuffleDeck()
        {
            if(Deck.Count >= 5)
            {
                Random rand = new Random();
                var shuffled = Deck.OrderBy(x => rand.Next()).ToList();
                Deck = shuffled;
            }
        }

        public void Mulligan(List<GameCard> list)
        {
            if (Deck.Count < list.Count)
                return;

            foreach(var card in list)
            {
                AddCardToDeck(card);
                DrawCard();
            }
        }

        public void MulliganAll()
        {
            if (Deck.Count < 5)
                return;

            foreach (var card in Hand)
            {
                AddCardToDeck(card);
                DrawCard();
            }
        }
    }
}
