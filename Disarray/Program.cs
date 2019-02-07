using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Disarray {
    class Game {
        public static void Main(string[] args) {
            Game g = new Game();
            g.Run();
        }
        public Dictionary<IPlayer, PlayerCard> cards { get; private set; }
        public List<UI> ui { get; private set; }
        public IPlayer[] players { get; private set; }
        public List<string> log { get; private set; }

        public Game(int playerCount = 2) {
            Console.CursorVisible = false;
            ui = new List<UI>();
            cards = new Dictionary<IPlayer, PlayerCard>();
            players = new IPlayer[playerCount];
            for (int i = 0; i < playerCount; i++) {
                cards[players[i] = new Human()] = new PlayerCard(players[i]);
            }
            log = new List<string>();
        }
        void Line(string message) {
            Console.WriteLine(message);
        }
        public void Update() {
            Console.Clear();
            foreach (PlayerCard c in cards.Values) {
                c.Print();
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            int i = 0;
            foreach (var u in ui) {
                u.Print();
                i++;
            }
            while(i < 10) {
                Console.WriteLine();
                i++;
            }
            log.ForEach(l => Console.WriteLine(l));
            Console.SetCursorPosition(0, 0);

        }
        void Run() {
            bool active = true;
            IPlayer last = null;

            while (active) {
                for (int i = 0; i < players.Length; i++) {
                    IPlayer p = players[i];
                    if (p.hp > 0) {
                        if (p == last) {
                            ui.Add(new Label($"Player {i + 1} won the war!"));
                            Update();
                            active = false;

                        } else {
                            last = p;
                            Label l = new Label($"It is Player {i + 1}'s turn.");
                            ui.Add(l);
                            p.Play(this);
                            ui.Remove(l);
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }
    interface UI {
        void Print();
    }
    class Label : UI {
        string s;
        public Label(string s) {
            this.s = s;
        }
        public void Print() {
            Console.WriteLine(s);
        }
    }
    class PlayerCard {
        public int? pointer;
        private IPlayer p;
        public PlayerCard(IPlayer p) {
            this.p = p;
        }
        public void Print() {
            int defenseCount = p.defenses.Length;
            string line2 = $" [ {string.Join(" ", p.defenses)} ] ";
            string line1 = $"{new string(' ', line2.Length / 2 - 2)}[{p.hp}]";
            line1 += new string(' ', line2.Length - line1.Length);

            int leftMargin = Console.CursorLeft;
            Console.Write(line1);
            (int top, int rightMargin) = (Console.CursorTop, Console.CursorLeft);
            (Console.CursorTop, Console.CursorLeft) = (Console.CursorTop + 1, leftMargin);
            Console.Write(line2);

            if (pointer != null) {
                string line3;
                if (pointer != -1) {
                    line3 = $" > {string.Join(" ", p.defenses.Select((d, i) => i == pointer ? "^" : " "))} < ";
                } else {
                    line3 = $" {new string('^', line2.Length - 2)} ";
                }

                (Console.CursorTop, Console.CursorLeft) = (Console.CursorTop + 1, leftMargin);
                Console.Write(line3);
            }
            (Console.CursorTop, Console.CursorLeft) = (top, rightMargin);

        }
    }
    class Dynamic : UI {
        Func<UI> source;
        public Dynamic(Func<UI> source) {
            this.source = source;
        }
        public void Print() {
            source.Invoke().Print();
        }
    }
    interface IPlayer {
        int hp { get; set; }
        int[] defenses { get; set; }
        void Play(Game game);
    }

    class Human : IPlayer {
        //Each player has hp and a wall of defenses
        //The number at each index represents the strength of the defense at that index
        //Players take turns managing defenses.
        //A player can choose between upgrading a defense or launching it at an enemy
        //Launching a defense resets it to zero and deals damage equal to its level to the target player's defense at the target index
        //Excess damage is taken by core hp

        public int hp { get; set; }
        public int[] defenses { get; set; }
        public Human() {
            hp = 16;
            defenses = new int[8];
        }
        private bool build(int index) {
            if (defenses[index] < 9) {
                defenses[index]++;
                return true;
            } else {
                return false;
            }
        }
        public void Damage(int index, int damage, out int extra) {
            ref int defense = ref defenses[index];
            if (defense >= damage) {
                defense -= damage;
                extra = 0;
            } else {
                defense = 0;
                extra = damage - defense;
            }
        }
        public void Play(Game game) {
            int actions = 1;
            Enter:
            {
                HashSet<UI> ui = new HashSet<UI>();
                ui.Add(new Dynamic(() => new Label($"Actions remaining: {actions}")));
                string[] menu = { "Build", "Attack" };
                int choice = 0;
                bool selected = false;
                for (int i = 0; i < menu.Length; i++) {
                    int index = i;
                    ui.Add(new Dynamic(() => new Label($"{(index == choice ? selected ? ">>" : ">" : "")}{menu[index]}")));
                }
                game.ui.AddRange(ui);
                Select:
                selected = false;
                game.cards[this].pointer = -1;
                game.Update();
                if (actions == 0) {
                    Cleanup();
                    return;
                }

                var key = Console.ReadKey();
                switch (key.Key) {
                    case ConsoleKey.RightArrow:
                        selected = true;
                        switch (choice) {
                            case 0:
                                actions--;
                                for(int i = 0; i < defenses.Length; i++) {
                                    build(i);
                                }
                                /*
                                Label prompt = new Label("Select the defense to build");
                                game.ui.Add(prompt);
                                SelectWall(i => {
                                    if(build(i)) {
                                        actions--;
                                    }
                                });
                                game.ui.Remove(prompt);
                                */
                                goto Select;
                            case 1: Attack(); goto Select;
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        choice++;
                        choice %= menu.Length;
                        goto Select;
                    case ConsoleKey.DownArrow:
                        choice--;
                        if (choice < 0) {
                            choice = menu.Length - 1;
                        }
                        goto Select;
                    default:
                        goto Select;
                }

                void Cleanup() {
                    game.cards[this].pointer = null;
                    game.ui.RemoveAll(u => ui.Contains(u));
                    game.Update();
                }
            }
            void SelectWall(Action<int> act) {
                int choice = 0;
                game.cards[this].pointer = choice;
                Select:
                if (actions == 0) {
                    goto Cleanup;
                }

                game.Update();
                var key = Console.ReadKey();
                switch (key.Key) {
                    case ConsoleKey.UpArrow:
                        act(choice);
                        game.cards[this].pointer = choice;
                        goto Select;
                    case ConsoleKey.RightArrow:
                        choice++;
                        choice %= defenses.Length;
                        game.cards[this].pointer = choice;
                        goto Select;
                    case ConsoleKey.LeftArrow:
                        choice--;
                        if (choice < 0) {
                            choice = defenses.Length - 1;
                        }
                        game.cards[this].pointer = choice;
                        goto Select;
                    case ConsoleKey.DownArrow:
                        break;
                    default:
                        goto Select;
                }
                Cleanup:
                game.cards[this].pointer = null;
            }
            /*
            void Build() {
                int choice = 0;
                game.cards[this].pointer = choice;
                Select:
                if (actions == 0) {
                    goto Cleanup;
                }

                game.Update();
                var key = Console.ReadKey();
                switch (key.Key) {
                    case ConsoleKey.Enter:
                        if (build(choice)) {
                            actions--;
                        }
                        goto Select;
                    case ConsoleKey.RightArrow:
                        choice++;
                        choice %= defenses.Length;
                        game.cards[this].pointer = choice;
                        goto Select;
                    case ConsoleKey.LeftArrow:
                        choice--;
                        if (choice < 0) {
                            choice = defenses.Length - 1;
                        }
                        game.cards[this].pointer = choice;
                        goto Select;
                    case ConsoleKey.Escape:
                        break;
                    default:
                        goto Select;
                }
                Cleanup:
                game.cards[this].pointer = null;
            }
            */
            void Attack() {
                Label prompt = new Label("Select the defense to launch");
                game.ui.Add(prompt);
                SelectWall(launched => {
                    if(defenses[launched] == 0) {
                        return;
                    }
                    game.cards[this].pointer = null;

                    Label prompt2 = new Label("Select the player to target");
                    game.ui.Add(prompt2);
                    int choice = 0;
                    SetPointer();
                    Select:
                    if (actions == 0) {
                        goto Cleanup;
                    }

                    game.Update();
                    var key = Console.ReadKey();
                    switch (key.Key) {
                        case ConsoleKey.UpArrow:
                            IPlayer target = game.players[choice];

                            if(target.hp == 0) {
                                goto Select;
                            }
                            ClearPointer();
                            Label prompt3 = new Label("Select the defense to target");
                            game.ui.Add(prompt3);
                            SelectTarget(target, targeted => {
                                actions--;
                                ref int attack = ref defenses[launched];
                                ref int defense = ref target.defenses[targeted];
                                /*
                                game.log.AddRange(new[] {
                                    $"Launched: {launched}",
                                    $"Launched HP: {defenses[launched]}",
                                    $"Attacked: {targeted}",
                                    $"Attacked HP: {target.defenses[targeted]}"
                                });
                                */
                                int extra;
                                if (defense >= attack) {
                                    defense -= attack;
                                    extra = 0;
                                } else {
                                    extra = attack - defense;
                                    defense = 0;
                                }
                                attack = 0;
                                target.hp -= extra;
                                if(target.hp < 1) {
                                    target.hp = 0;
                                    actions = 0;
                                }
                                /*
                                game.log.AddRange(new[] {
                                    $"Launched: {launched}",
                                    $"Launched HP: {defenses[launched]}",
                                    $"Attacked: {targeted}",
                                    $"Attacked HP: {target.defenses[targeted]}"
                                });
                                */
                            });
                            game.ui.Remove(prompt3);
                            SetPointer();
                            break;
                        case ConsoleKey.RightArrow:
                            ClearPointer();
                            choice++;
                            choice %= game.players.Length;
                            SetPointer();
                            goto Select;
                        case ConsoleKey.LeftArrow:
                            ClearPointer();
                            choice--;
                            if (choice < 0) {
                                choice = game.players.Length - 1;
                            }
                            SetPointer();
                            goto Select;
                        case ConsoleKey.DownArrow:
                            break;
                        default:
                            goto Select;
                    }

                    Cleanup:
                    game.ui.Remove(prompt2);
                    foreach (var card in game.cards.Values) {
                        card.pointer = null;
                    }
                    void SetPointer() {
                        game.cards[game.players[choice]].pointer = -1;
                    }
                    void ClearPointer() {
                        game.cards[game.players[choice]].pointer = null;
                    }
                    void SelectTarget(IPlayer player, Action<int> act){
                        int choice2 = 0;
                        game.cards[player].pointer = choice2;
                        Select2:
                        game.Update();
                        var key2 = Console.ReadKey();
                        switch (key2.Key) {
                            case ConsoleKey.UpArrow:
                                act(choice2);
                                break;
                            case ConsoleKey.RightArrow:
                                choice2++;
                                choice2 %= player.defenses.Length;
                                game.cards[player].pointer = choice2;
                                goto Select2;
                            case ConsoleKey.LeftArrow:
                                choice2--;
                                if (choice2 < 0) {
                                    choice2 = player.defenses.Length - 1;
                                }
                                game.cards[player].pointer = choice2;
                                goto Select2;
                            case ConsoleKey.DownArrow:
                                break;
                            default:
                                goto Select2;
                        }
                        game.cards[player].pointer = null;
                    }
                });
                game.ui.Remove(prompt);
            }
        }
    }
}
