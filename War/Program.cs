namespace WarGame
{
    class Programm
    {
        static void Main()
        {
            Menu menu = new Menu();
            menu.Run();
        }
    }

    class Menu
    {
        private const string StartFight = "1";
        private const string MenuExit = "0";
        private Battlefield _battlefield = new Battlefield();

        public void Run()
        {
            bool isExit = false;
            string userInput;

            while (isExit == false)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine(StartFight + " - Начать бой!");
                Console.WriteLine(MenuExit + " - Выход");

                userInput = Console.ReadLine();

                switch (userInput)
                {
                    case StartFight:
                        _battlefield.PerformBattle();
                        break;

                    case MenuExit:
                        isExit = true;
                        break;
                }
            }
        }
    }

    class Battlefield
    {
         public void PerformBattle()
        {
            Console.WriteLine("Введите название первой страны");
            Squad _country1 = new Squad(Console.ReadLine());
            Console.WriteLine("Введите название второй страны");
            Squad _country2 = new Squad(Console.ReadLine());

            while (_country1.IsAllDead == false && _country2.IsAllDead == false) 
            {
                _country2.TakeDamage(_country1.CreateAttackTable(_country2.GiveAliveIndex()));
                Console.WriteLine("___________________________________________________________________________________________________________________________");
                _country1.TakeDamage(_country2.CreateAttackTable(_country1.GiveAliveIndex()));
                Console.WriteLine("___________________________________________________________________________________________________________________________");
            }
        }
    }

    class Squad
    {
        private List<Soldier> _army;
        public string CountryName;
        private int _currentIndex = 0;

        public Squad(string countryName)
        {
            CountryName = countryName;
            _army = CreateArmy();
        }

        public bool IsAllDead { get; private set; } = false;

        private List<Soldier> CreateArmy()
        {
            List<Soldier> squad = new List<Soldier>();
            Random random = new Random();
            int armySize = 100;

            Soldier[] soldiers = new Soldier[]
            {
                new Trooper(),
                new Marksman(),
                new MachineGunner(),
            };

            for (int i = 0; i < armySize; i++)
            {
                Soldier soldier = soldiers[random.Next(0, soldiers.Length)];
                soldier.Index = _currentIndex++;
                squad.Add(soldier);
            }

            return squad;
        }

        public void TakeDamage(List<int[,]> attackTable)
        {
            int[,] damageTable;
            int index;
            int damage;

            if (attackTable != null)
            {
                for (int i = 0; i < attackTable.Count(); i++)
                {
                    damageTable = attackTable[i];
                    
                    for (int j = 0; j < damageTable.GetLength(0); j++) 
                    {
                        Soldier foundSoldier = _army.Find(soldier => soldier.Index == damageTable[j, 0]);

                        if (foundSoldier != null) 
                        {
                            foundSoldier.TakeDamage(damageTable[j, 1]);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Страна {CountryName} победила!");
                IsAllDead = true;
            }
        }

        public List<int[,]> CreateAttackTable(int[] aliveSoldiers)
        {
            List <int[,]> attackTable = new List<int[,]>(); //т.к безразмерный двумерный массив не создать, используем лист.
            int armyHealth = 0;

            for (int i = 0; i < _army.Count; i++)
            {
                if (_army[i].Health > 0)
                {
                    _army[i].AttackTargets(aliveSoldiers);
                    int[,] targets = _army[i].TargetsTable;
                    attackTable.Add(targets);
                }
            }
            for (int i = 0; i < _army.Count(); i++)
            {
                armyHealth += _army[i].Health;
            }
            if (armyHealth > 0)
            {
                return attackTable;
            }
            else
            {
                return null;
            }
        }

        public int[] GiveAliveIndex() //Предоставляет массив с индексами (уникальными номерами) живых солдат в которых можно стрелять и свободной ячейкой для получения урона.
        {
            int[] aliveSoldiers = new int[_army.Count];

            for (int i = 0; i < aliveSoldiers.Length; i++)
            {
                if (_army[i].Health > 0)
                {
                    aliveSoldiers[i] = _army[i].Index;
                }
            }

            return aliveSoldiers;
        }
    }

    class Soldier
    {
        public int Index;
        protected int Armor;
        protected int HitDamage;
        protected int BaseDamage = 15;
        protected int TargetsAvailable;
        protected static Random Random = new Random();

        public Soldier(int index = 0)
        {
            Index = index;
            Health = 500;
            Armor = 30;
            HitDamage = 100;
            TargetsAvailable = 1;
            TargetsTable = new int[TargetsAvailable, 2];
        }

        public int[,] TargetsTable { get; private set; }
        public int Health { get; protected set; }

        public int[,] AttackTargets(int[] aliveSoldiers)
        {
            ChooseTarget(aliveSoldiers);

            for (int i = 0; i < TargetsTable.Length - 1; i++)
            {
                TargetsTable[i, 1] = CalculateDamage(); //Выходной урон
            }

            return TargetsTable;
        }

        protected virtual int CalculateDamage()
        {
            int alternateDamage = UseAttackAbility();

            double minDamageMod = 0.8;
            double maxDamageMod = 1.2;
            double damageMod = minDamageMod + (Random.NextDouble() * (maxDamageMod - minDamageMod));

            if (alternateDamage != 0)
            {
                int damage = alternateDamage;
                Console.Write($"Солдат {Index} Пытается нанести {damage} урона ");
                return damage;
            }
            else
            {
                int damage = (int)(HitDamage * damageMod);
                Console.Write($"Солдат {Index} Пытается нанести {damage} урона ");
                return damage;
            }
        }

        public virtual void TakeDamage(int damage)
        {
            int reducedDamage = damage - Armor;

            if (reducedDamage < BaseDamage)
            {
                reducedDamage = BaseDamage;
            }
            else
            {
                Health -= reducedDamage;
            }

            Console.WriteLine($"Солдат {Index} получает {reducedDamage} урона, остаётся {Health} хп.");
        }

        protected virtual int UseAttackAbility()
        {
            return 0;
        }

        private void ChooseTarget(int[] aliveSoldiers) //случайный выбор цели
        {
            for (int i = 0 ; i < TargetsTable.Length - 1; i++)
            {
                int tempIndex = Random.Next(0, aliveSoldiers.Length); // получаем случайную ячейку массива
                TargetsTable[i,0] = aliveSoldiers[tempIndex]; //записываем индекс цели в таблицу урона солдата
            }
        }
    }

    class Trooper : Soldier
    {
        public Trooper()
        {
            Health = 500;
            Armor = 30;
            HitDamage = 70;
        }
    }

    class Marksman : Soldier
    {
        public Marksman()
        {
            Health = 500;
            Armor = 0;
            HitDamage = 100;
            TargetsAvailable = 2;
        }

        protected override int UseAttackAbility()
        {
            double critMultiplier = 3;
            int critChance = 25;
            int CriticalShot = HitDamage;

            if (Random.Next(0, 100) < critChance)
            {
                CriticalShot = Convert.ToInt32(Math.Round(CriticalShot * critMultiplier));
                Console.WriteLine($"Солдат {Index} наносит критический урон!");
            }

            return CriticalShot;
        }
    }

    class MachineGunner : Soldier
    {
        public MachineGunner()
        {
            int minTargets = 1;
            int maxTargets = 5;
            Health = 500;
            Armor = 15;
            HitDamage = 70;
            TargetsAvailable = Random.Next(minTargets, maxTargets);
        }
    }
}