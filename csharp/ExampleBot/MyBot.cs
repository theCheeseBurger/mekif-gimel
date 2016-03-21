using Pirates;
using System.Collections.Generic;
using System;

namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        //maximum moves
        static int Maxmoves = 6;

        //TODO: useless
        static bool hastMaxmoves = true;

        //doing mathemics
        static bool isMathworks = true;

        //attackMod
        static bool attackMod = false;

        //forbidden treasures.
        static List<TakenTreasure> forbiddenTreasures = new List<TakenTreasure>();

        internal class BoardStatus
        {
            public List<Pirate> Pirates { get; set; }
            public List<Treasure> Treasures { get; set; }
        }

        internal class PirateTactics
        {
            public Pirate Pirate { get; set; }
            public Location FinalDestination { get; set; }
            public Location TempDestination { get; set; }
            public int Moves { get; set; }

        }

        internal class TakenTreasure
        {
            public TakenTreasure(Pirate pirate, Treasure treasure)
            {
                this.pirate = pirate;
                this.Treasure = treasure;
            }
            public Pirate pirate { get; set; }
            public Treasure Treasure { get; set; }
        }

        public void DoTurn(IPirateGame game)
        {
            Maxmoves = 6;
            isMathworks = true;
            hastMaxmoves = true;
            forbiddenTreasures = new List<TakenTreasure>();

            BoardStatus status = GetBoardStatus(game);
            List<PirateTactics> tactics = AssignTargets(game, status);
            if (tactics != null)
            {
                for (int i = 0; i < tactics.Count; i++)
                {
                    TakeAction(game, tactics[i]);
                }


            }
        }

        private BoardStatus GetBoardStatus(IPirateGame game)
        {
            BoardStatus br = new BoardStatus();
            br.Pirates = new List<Pirate>();
            for (int i = 0; i < game.MyPirates().Count; i++)
            {
                br.Pirates.Add(game.MyPirates()[i]);
            }
            return br;
        }

        private List<PirateTactics> AssignTargets(IPirateGame game, BoardStatus status)
        {

            List<PirateTactics> lst = new List<PirateTactics>();
            List<Pirate> pirates = game.MySoberPirates();
            for (int i = 0; i < pirates.Count; i++)
            {
                PirateTactics pt = pirateTarget(game, pirates[i]);
                if (pt != null)
                {
                    lst.Add(pt);
                }
            }
            return lst;
        }

        private void TakeAction(IPirateGame game, PirateTactics tactics)
        {
            if (tactics != null && game.Treasures() != null)
            {
                if (attackMod && TryAttack(game, tactics.Pirate))
                {
                    return;
                }
                game.SetSail(tactics.Pirate, tactics.TempDestination);
            }


        }

        /// <summary>
        /// gets the closest treaure for the priate.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns></returns>
        private Treasure getMinTreaure(IPirateGame game, Pirate pirate)
        {

            List<Treasure> lst = game.Treasures();
            List<Location> lc;
            int i = 1;
            Treasure tr = lst[lst.Count - 1];
            int mindst = 1000;

            foreach (Treasure item in lst)
            {

                lc = game.GetSailOptions(pirate, item.Location, i);

                while (lc.Count == 0)
                {
                    i++;
                    lc = game.GetSailOptions(pirate, item.Location, i);
                }

                if (i < mindst)
                {
                    mindst = i;
                    tr = item;
                }

            }
            return tr;



        }

        /// <summary>
        /// find the closest treasure
        /// </summary>
        /// <param name="game"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Treasure findTreasure(IPirateGame game, Pirate pirate)
        {
            foreach (TakenTreasure tt in forbiddenTreasures)
            {
                if (tt.pirate == pirate)
                {
                    return tt.Treasure;
                }
            }

            List<Treasure> lst = game.Treasures();
            if (game.Treasures().Count == 0)
            {
                return null;
            }
            int minmovements = game.Distance(lst[0].Location, pirate.Location);
            Treasure minTreasure = lst[0];

            for (int i = 1; i < lst.Count; i++)
            {
                int mv = game.Distance(lst[i].Location, pirate.Location);
                if (mv < minmovements && !isForbiddenTreasure(game, lst[i]))
                {
                    minmovements = mv;
                    minTreasure = lst[i];
                }

            }

            forbiddenTreasures.Add(new TakenTreasure(pirate, minTreasure));
            return minTreasure;
        }

        /// <summary>
        /// target a pirate location, and return the pirates tactic, require the game, the status of the game, and the id of the pirate to target .
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="status">status of the game</param>
        /// <param name="id">id of the pirate</param>
        /// <returns></returns>
        private PirateTactics pirateTarget(IPirateGame game, Pirate pirate)
        {

            PirateTactics tactics = new PirateTactics() { Pirate = pirate };

            // //priorities..
            // if (game.Treasures().Count > 0 && game.Treasures().Count <= game.MyPiratesWithoutTreasures().Count)
            // {
            //     tactics.Moves = PriorityGoldMoves(game, pirate);
            // }
            //else
            //  {
            //     tactics.Moves = PriorityDistributionMoves(game, tactics.Pirate);
            // }
            tactics.Moves = PriorityGoldMoves(game, pirate);

            //debugging - shows nicely each pirate and its move for the turn.
            game.Debug("-> pirate id : " + tactics.Pirate.Id);
            game.Debug("-> destination : " + tactics.FinalDestination);
            game.Debug("-> moves : " + tactics.Moves);
            game.Debug("-------------------------");


            if (tactics.Moves != 0)
            {
                //for -1 reference, see distribtionGold
                if (tactics.Moves == -1)
                {
                    tactics.FinalDestination = tactics.Pirate.InitialLocation;
                    tactics.Moves = 1;
                }
                else
                {
                    Pirate enemyPirate = isWorthtoAttack(game, pirate, findTreasure(game, pirate));
                    if (attackMod && enemyPirate != null)
                    {
                        tactics.FinalDestination = enemyPirate.Location;
                        game.Debug("happend!!");

                    }
                    else
                    {
                        tactics.FinalDestination = findTreasure(game, pirate).Location;
                    }
                }

                List<Location> possibleLocations =
                        game.GetSailOptions(tactics.Pirate,
                                            tactics.FinalDestination,
                                            tactics.Moves);


                tactics.TempDestination = possibleLocations[0];


                return tactics;
            }

            return null;
        }

        /// <summary>
        /// calculating maxmoves that the pirate can do.(for having every boat to move).
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns>return a number between 0-maxmoves (depends on the api)</returns>
        private int PriorityDistributionMoves(IPirateGame game, Pirate pirate)
        {
            //checks for any treasures, if no treasure founds, just returns all the ships with treasures back to home.
            if (game.Treasures().Count > 0)
            {
                //if pirate has no treasure, doing mathematics for caluclaling that all ships will move in maximum effort.
                int _Maxmoves = 0;

                if (!pirate.HasTreasure)
                {
                    int leftover = (Maxmoves - game.MyPiratesWithTreasures().Count) % (game.MyPiratesWithoutTreasures().Count - numOfAttackers(game));
                    if (leftover != 0)
                    {
                        _Maxmoves = (Maxmoves - game.MyPiratesWithTreasures().Count) / (game.MyPiratesWithoutTreasures().Count - numOfAttackers(game));
                        if (isMathworks)
                        {
                            _Maxmoves += leftover;
                            //does this only one time a turn.
                            isMathworks = false;
                        }
                    }
                    else
                    {
                        _Maxmoves = (Maxmoves - game.MyPiratesWithTreasures().Count) / (game.MyPiratesWithoutTreasures().Count - numOfAttackers(game));
                    }
                    return _Maxmoves;
                }
            }
            if (pirate.HasTreasure)
            {
                //its a trick!, if its -1 , ill know in the location, that it will need to go back!
                return -1;
            }
            //TODO: fix this
            return -1;

        }

        /// <summary>
        /// priority for the last gold ( because you want to pick up the last few golds).. will give the ones who are grabbing the gold 0 and those without gold max.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns></returns>
        private int PriorityGoldMoves(IPirateGame game, Pirate pirate)
        {
            if (pirate.TurnsToSober != 0)
            {
                return 0;
            }
            if (pirate.HasTreasure)
            {
                return -1;
            }
            if (minDistancePerStatus(game) == pirate)
            {
                return Maxmoves - game.MyPiratesWithTreasures().Count;
            }
            return 0;


        }

        /// <summary>
        /// gets the minimum location between set of locations and a pirate.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="locations"></param>
        /// <param name="pirate"></param>
        /// <returns></returns>
        /*private Location minLocation(IPirateGame game ,List<Location> locations, Pirate pirate)
        {

            Location minLocation = locations[0];
            int minDistance = game.Distance(pirate, minLocation);

            int distance;
            int id = 0;
            
            for (int i = 0; i < locations.Count; i++)
			{
                distance = game.Distance(pirate, locations[i]);
                if(distance < minDistance)
                {
                    minDistance = distance;
                    minLocation = locations[i];
                    id = i;
                }
            }

            return minLocation;*/

        /// <summary>
        /// returns the enemey pirate if is worth to attack the pirate else if no pirate is found returns null.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <param name="treasure"></param>
        /// <returns></returns>
        /// TODO: לשנות את השם
        private Pirate isWorthtoAttack(IPirateGame game, Pirate pirate, Treasure treasure)
        {
            if (treasure == null)
            {
                //TODO: taktiakt tkifa!
                return null;
            }
            List<Pirate> pirates = game.EnemySoberPirates();
            Pirate enemeyPirate = null;
            foreach (Pirate ePirate in pirates)
            {

                //TODO: check if moves is correleted to the number of reload turns - not the best
                //TODO: לא צריך לבדוק שוב אם הם שיכורים 
                //TODO: לשנות את הקטן לגדול כשמשווים מרחקים
                if (ePirate.TurnsToSober == 0 && game.Distance(pirate, treasure) <= game.Distance(ePirate, treasure) && pirate.ReloadTurns == 0)
                {
                    enemeyPirate = ePirate;
                }
            }
            return enemeyPirate;
        }

        private bool TryAttack(IPirateGame game, Pirate pirate)
        {
            foreach (Pirate enemy in game.EnemyPirates())
            {
                if (game.InRange(pirate, enemy) && pirate.ReloadTurns == 0)
                {
                    game.Attack(pirate, enemy);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// retuns the number of attacking ships we control.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private int numOfAttackers(IPirateGame game)

        {
            int counter = 0;
            //TODO: רק רשימת הפיארטים שלא שיכורים
            foreach (Pirate pirate in game.AllMyPirates())
            {
                if (attackMod && pirate.TurnsToSober != 0 && isWorthtoAttack(game, pirate, findTreasure(game, pirate)) != null)
                {
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        /// gets the pirate which its distance to the closest treature is the minimum
        /// </summary>
        /// <param name="game">the game</param>
        /// <returns>the pirate with the characaristics we said</returns>
        public Pirate minDistancePerStatus(IPirateGame game)
        {
            if (MyGoodPirates(game).Count == 0)
            {
                return null;
            }
            Pirate minPirate = MyGoodPirates(game)[0];
            if (findTreasure(game, minPirate) == null)
            {
                return null;
            }
            int minDistance = game.Distance(minPirate, findTreasure(game, minPirate));
            foreach (Pirate pirate in MyGoodPirates(game)) 
            {
                if (minDistance > game.Distance(pirate, findTreasure(game, pirate)))
                {
                    minDistance = game.Distance(pirate, findTreasure(game, pirate));
                    minPirate = pirate;
                }
            }
            game.Debug("ITS OK " + minPirate.Id);
            return minPirate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns>returns the sober pirates that has no trasure</returns>
        public List<Pirate> MyGoodPirates(IPirateGame game)
        {
            List<Pirate> withoutSober = new List<Pirate>();
            foreach (Pirate pirate in game.MySoberPirates())
            {
                if (game.MyPiratesWithoutTreasures().Contains(pirate))
                {
                    withoutSober.Add(pirate);
                }
            }
            return withoutSober;
        }

        public bool isForbiddenTreasure(IPirateGame game, Treasure treasure)
        {
            foreach (TakenTreasure tr in forbiddenTreasures)
            {
                if (tr.Treasure == treasure)
                {
                    return true;
                }
            }
            return false;
        }
        public Pirate IsAttack (IPirateGame game, Pirate pirate)
        {
            foreach (Pirate enemy in game.EnemyPirates())
            {
                if (game.InRange(pirate, enemy))
                {
                    game.Attack(pirate, enemy);
                    return enemy;
                }
            }
            return null;
        }
        #region helpfull

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public List<Pirate> EnemyGoodPirates(IPirateGame game)
        {
            //מביא לי את כל האויבים הטובים שהם לא שיכורים ולא אבודים
            List<Pirate> EnemyWithoutSober = new List<Pirate>();

            foreach (Pirate pirate in game.EnemySoberPirates())
            {
                if (game.EnemyPiratesWithoutTreasures().Contains(pirate))
                {
                    EnemyWithoutSober.Add(pirate);
                }
            }
            return EnemyWithoutSober;
        }
        #endregion
        #region Always
        /// <summary>
        /// checks if enemy is loaded and closer than the treasure
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns></returns the best enemy>
        private Pirate IsEnemyCloseAndLoad(IPirateGame game, Pirate pirate)
        {
            // הפעולה מקבלת את המשחק ופיראט, 
            //ואם האויב הוא קרוב יותר אליך מאשר לאוצר והוא גם טעון אז הוא שווה תקיפה
            List<Pirate> soberEnemy = EnemyGoodPirates(game);
            foreach (Pirate enemypirete in soberEnemy)
            {
                if (game.Distance(enemypirete, pirate) < game.Distance(pirate, getMinTreaure(game, pirate)) && enemypirete.ReloadTurns == 0)
                {
                    return enemypirete;
                }
            }
            return null;
        }
        /// <summary>
        /// checks if enemy if you closer to the enemy with treasure than the treasure
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns></returns the best enemy>
        private Pirate ThiefTactic(IPirateGame game, Pirate pirate)
        {
            List<Pirate> enemtWithTreasures = game.EnemyPiratesWithTreasures();
            foreach (Pirate enemyPirete in enemtWithTreasures)
            {
                //מקבל משחק ופיראט ובודק אם האויב שקרוב יותר לאוצר וגם יש לו אוצר אז שווה תקיפה
                if (game.Distance(enemyPirete, pirate) < game.Distance(pirate, getMinTreaure(game, pirate))
                    && enemyPirete.HasTreasure)
                {
                    return enemyPirete;
                }
            }
            return null;
        }
        #endregion
        #region helpfull 
        /// <summary>
        /// return true if we lose
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns true if we lose else false>
        private bool Islose(IPirateGame game)
        {
            if ((game.GetMyScore() + game.MyPiratesWithTreasures().Count) < (game.GetEnemyScore() + game.EnemyPiratesWithTreasures().Count))
            {
                return true;
            }
            return false;
        }
        #endregion
        #region End Game

        private Pirate LastChance(IPirateGame game, Pirate pirate)
        {
            List<Pirate> myPirate = MyGoodPirates(game);
            List<Pirate> enemyPirate = game.EnemySoberPirates();
            if (Islose(game))
            {
                foreach (Pirate enemy in enemyPirate)
                {
                    foreach (Pirate friend in myPirate)
                    {
                        if (enemy.HasTreasure && game.Distance(enemy, enemy.InitialLocation) <= game.Distance(enemy , friend))
                        {
                            return enemy;
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }

}