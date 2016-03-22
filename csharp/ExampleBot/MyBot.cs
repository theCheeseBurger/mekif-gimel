﻿using Pirates;
using System.Collections.Generic;
using System;

namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        //maximum moves
        static int Maxmoves = 6;
        static bool hastMaxmoves = true; //TODO: unnecessary

        //TODO: doing mathemics change name to extra moves
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

        //REMOVE
        private BoardStatus GetBoardStatus(IPirateGame game)
        {
            BoardStatus br = new BoardStatus();
            //name

            br.Pirates = new List<Pirate>();
            for (int i = 0; i < game.MyPirates().Count; i++)
            {
                br.Pirates.Add(game.MyPirates()[i]);
            }
            //doesnt return treasures
            return br;
        }

        //TO MAIN
        private List<PirateTactics> AssignTargets(IPirateGame game, BoardStatus status)
        {

            List<PirateTactics> lst = new List<PirateTactics>();
            //TODO: change lst - not readable

            List<Pirate> pirates = game.MySoberPirates();
            //change pirates - not readable
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

        private Pirate findEnemyWithTreasure(IPirateGame game, Pirate pirate)
        {
            if (game.EnemyPiratesWithTreasures().Count > 0)
            {
                Pirate closestEnemy = game.EnemyPiratesWithTreasures()[0];
                foreach (Pirate enemy in game.EnemyPiratesWithTreasures())
                {
                    if (game.Distance(pirate, enemy) < game.Distance(pirate, closestEnemy))
                    {
                        closestEnemy = enemy;
                    }
                }
                return closestEnemy;
            }
            return null;
        }

        private List<Pirate> EnemyGoodPirates(IPirateGame game)
        {
            List<Pirate> goodPirates = new List<Pirate>();
            foreach (Pirate enemy in game.EnemySoberPirates())
            {
                if (!enemy.HasTreasure)
                    goodPirates.Add(enemy);
            }
            return goodPirates;
        }

        private bool TryDefence(IPirateGame game, Pirate friendlyPirate)
        {
            foreach (Pirate enemy in game.EnemyPirates())
            {
                if (game.InRange(friendlyPirate, enemy) && enemy.ReloadTurns == 0 && !enemy.HasTreasure)
                {
                    game.Defend(friendlyPirate);
                    return true;
                }
            }
            return false;
        }

        private void TakeAction(IPirateGame game, PirateTactics tactics)
        {
            if (tactics != null && game.Treasures().Count >= 0)
            {
                Pirate enemy = findEnemyWithTreasure(game, tactics.Pirate);
                if (enemy != null)
                {

                    if (tactics.Pirate.HasTreasure)
                    {
                        foreach (Pirate enemyPirate in EnemyGoodPirates(game))
                        {
                            if (game.InRange(tactics.Pirate, enemyPirate) && enemyPirate.ReloadTurns == 0)
                            {
                                game.Defend(tactics.Pirate);
                                return;
                            }
                        }
                    }
                    if (game.InRange(tactics.Pirate, enemy))
                    {
                        game.Attack(tactics.Pirate, enemy);
                        return;
                    }
                    game.SetSail(tactics.Pirate, tactics.TempDestination);
                    return;
                }
                game.SetSail(tactics.Pirate, tactics.TempDestination);
            }
        }

        //REMOVE
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
        //TODO: name
        {
            foreach (TakenTreasure tt in forbiddenTreasures)
            {
                //TODO: nameeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (tt.pirate == pirate)
                {
                    return tt.Treasure;
                }
            }

            List<Treasure> lst = game.Treasures();
            //TODO: name
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
                //TODO: not very good if's
                if (tactics.Moves == -1)
                {
                    tactics.FinalDestination = tactics.Pirate.InitialLocation;
                    tactics.Moves = 1;/**/
                }
                else
                {
                    Pirate enemyPirate = findEnemyWithTreasure(game, pirate);
                    if (enemyPirate != null && pirate.ReloadTurns == 0)
                    {
                        tactics.FinalDestination = enemyPirate.Location;
                        game.Debug("happend!!");

                    }
                    else
                    {
                        tactics.FinalDestination = findTreasure(game, pirate).Location;
                    }
                }

                //List<Location> possibleLocations =
                //        game.GetSailOptions(tactics.Pirate,
                //                            tactics.FinalDestination,
                //                            tactics.Moves);


                //tactics.TempDestination = possibleLocations[0];
                NoCollisionOnlyDrunk(game, tactics);

                if (tactics.Moves != 0)
                {
                    return tactics;
                }
                return null;
            }
            //returns null when the pirate does not move.
            return null;
        }


        //NOT FUNCTION UNTIL FIXED ADAIAN LO HISTAMASHNOO OZ AMAR
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
                //its a trick!, if its -1 , ill know in the location, that it will need to go back to base.
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

        //CAHNGE NAME
        /// <summary>
        /// returns the enemey pirate if is worth to attack the pirate else if no pirate is found returns null.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <param name="treasure"></param>
        /// <returns></returns>
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
            foreach (Pirate pirate in game.AllMyPirates())
            {
                if (attackMod && pirate.TurnsToSober == 0 && isWorthtoAttack(game, pirate, findTreasure(game, pirate)) != null)
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
        private Pirate minDistancePerStatus(IPirateGame game)
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
        private List<Pirate> MyGoodPirates(IPirateGame game)
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

        private bool isForbiddenTreasure(IPirateGame game, Treasure treasure)
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

        private Pirate CloseToFriendPirate(IPirateGame game, Pirate pirate)
        {
            int distance;
            Pirate minLocationPirate = null;
            List<Pirate> closeToBasePirates = new List<Pirate>();
            int checkeddistance = game.Distance(pirate, pirate.InitialLocation);


            foreach (Pirate friendPirate in MyGoodPirates(game))
            {
                distance = game.Distance(friendPirate, pirate.InitialLocation);

                if (distance < checkeddistance)
                {
                    closeToBasePirates.Add(friendPirate);
                }

            }

            distance = int.MaxValue;

            foreach (Pirate closePirate in closeToBasePirates)
            {
                if (game.Distance(closePirate, pirate) < distance)
                {
                    distance = game.Distance(closePirate, pirate);
                    minLocationPirate = closePirate;
                }
            }

            return minLocationPirate;
        }

        public Treasure FarFromEnemiesTrs(IPirateGame game, Pirate pirate)
        {
            int sumofDistance;
            List<int> treasureDistances = new List<int>();
            List<Treasure> SortedTreasuresByDisFromEnemies = new List<Treasure>();


            foreach (Treasure treasure in game.Treasures())
            {
                sumofDistance = 0;

                foreach (Pirate ePirate in game.EnemySoberPirates())
                {
                    sumofDistance += game.Distance(ePirate, treasure);
                }
                treasureDistances.Add(sumofDistance);

            }

            return null;
        }
        public List<Treasure> GoodTreasures(IPirateGame game)
        {
            return null;
        }
        private void NoCollisionOnlyDrunk(IPirateGame game, PirateTactics tactic)
        {
            List<Location> locations = game.GetSailOptions(tactic.Pirate, tactic.FinalDestination, tactic.Moves);
            foreach (Pirate ePirate in game.EnemyDrunkPirates())
            {
                locations.Remove(ePirate.Location);
            }
            foreach (Pirate pirate in game.MyDrunkPirates())
            {
                locations.Remove(pirate.Location);
            }



            if (locations.Count == 0 && tactic.Moves != 0)
            {
                tactic.Moves -= 1;
                NoCollisionOnlyDrunk(game, tactic);
            }

            tactic.TempDestination = locations[0];
        }
    
    }
}