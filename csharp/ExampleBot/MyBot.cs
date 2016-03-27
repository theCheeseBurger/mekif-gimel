﻿﻿using Pirates;
using System.Collections.Generic;
using System;

namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {

        //biglal she takeAction kore ahrey NoCollision - Yahol lihiot hitnagshoot.

        static int Maxmoves;
        static bool isMovesLeft;
        static bool isMathworks;


        //added
        static List<Treasure> targetableTreasures;
        static List<Pirate> targetablePirates;

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
            public DestinationTarget destinationtarget { get; set; }
            public int Moves { get; set; }

            public string whichTarget()
            {
                string st;
                switch (destinationtarget)
                {
                    case DestinationTarget.toInitial: st = "to inital";
                        break;
                    case DestinationTarget.toPowerUp: st = "to power up";
                        break;
                    case DestinationTarget.toTreasure: st = "to treasure";
                        break;
                    case DestinationTarget.toAttack: st = "to attack";
                        break;
                    case DestinationTarget.toDefence: st = "to defence";
                        break;
                    case DestinationTarget.toCollide: st = "to collide";
                        break;
                    case DestinationTarget.toPirateInInital: st = "to pirate in intial";
                        break;
                    default: st = "error";
                        break;

                }
                return st;
            }

        }

        public enum DestinationTarget { toInitial, toPowerUp, toTreasure, toAttack, toDefence, toCollide, toPirateInInital }

        public void DoTurn(IPirateGame game)
        {

            game.Debug("max moves" + game.GetActionsPerTurn());
            isMathworks = true;
            targetablePirates = new List<Pirate>();
            foreach (Pirate ePirate in game.EnemySoberPirates())
            {
                targetablePirates.Add(ePirate);
            }

            //changed.
            Maxmoves = game.GetActionsPerTurn();
            isMovesLeft = true;
            //Addded.
            PirateTactics piratetactic;
            targetableTreasures = new List<Treasure>();
            List<PirateTactics> tactics = new List<PirateTactics>();
            foreach (Treasure treasure in game.Treasures())
            {
                if (game.GetPirateOn(treasure.Location) == null)
                {
                    targetableTreasures.Add(treasure);
                }
            }

            foreach (Pirate soberPirate in game.MySoberPirates())
            {
                piratetactic = GetPirateTarget(game, soberPirate);
                tactics.Add(piratetactic);
            }

            RemoveNulls(tactics);
            NoCollision(game, tactics);

            foreach (PirateTactics tactic in tactics)
            {
                if (tactic != null && tactic.Moves >= 0 && tactic.FinalDestination != null)
                {
                    TakeAction(game, tactic);
                }
            }
        }

        //TODO: NOT FINISHED - IT IS USING TRY DEFENCE AND TRY ATTACK - yesh lehosif enum.
        private void NewPriorityGoldMoves(IPirateGame game, List<PirateTactics> tactics)
        {
            int movesleft = Maxmoves - game.MyPiratesWithTreasures().Count;
            int maxdistnace;
            int distance;
            PirateTactics maxTactic;
            Pirate enemy;
            LinkedList<PirateTactics> sortedlinkedtactics = new LinkedList<PirateTactics>();


            int index = 0;
            while (index < tactics.Count)
            {
                enemy = findEnemyWithTreasure(game, tactics[index].Pirate);
                if (tryDefence(game, tactics[index].Pirate, false) || (enemy != null && game.InRange(tactics[index].Pirate, enemy)))
                {
                    sortedlinkedtactics.AddLast(tactics[index]);
                    tactics.Remove(tactics[index]);
                }
                else
                {
                    index++;
                }
            }



            //foreach (PirateTactics tactic in tactics)
            //{
            //    enemy = findEnemyWithTreasure(game, tactic.Pirate);
            //    if(tryDefence(game, tactic.Pirate, false) || (enemy != null && game.InRange(tactic.Pirate, enemy)))
            //    {
            //        sortedlinkedtactics.AddLast(tactic);
            //        tactics.Remove(tactic);
            //    }
            //}


            while (tactics.Count > 0)
            {
                maxdistnace = int.MinValue;
                maxTactic = null;
                index = 0;

                while (index < tactics.Count)
                {
                    distance = game.Distance(tactics[index].Pirate, tactics[index].FinalDestination);
                    if (distance > maxdistnace)
                    {
                        maxdistnace = distance;
                        maxTactic = tactics[index];
                    }
                    else
                    {
                        index++;
                    }
                }
                sortedlinkedtactics.AddFirst(maxTactic);
                tactics.Remove(maxTactic);
            }

            //while (tactics.Count > 0)
            //{
            //    maxdistnace = int.MinValue;
            //    maxTactic = null;

            //    foreach (PirateTactics tactic in tactics)
            //    {
            //        distance = game.Distance(tactic.Pirate, tactic.FinalDestination);
            //        if (distance > maxdistnace && tactic.destinationtarget != DestinationTarget.toPowerUp)
            //        {
            //            maxdistnace = distance;
            //            maxTactic = tactic;
            //        }
            //    }

            //    sortedlinkedtactics.AddFirst(maxTactic);
            //    tactics.Remove(maxTactic);
            //}

            ////powerups
            //foreach (PirateTactics tactic in tactics)
            //{
            //    sortedlinkedtactics.AddFirst(tactic);
            //}

            foreach (PirateTactics tactic in sortedlinkedtactics)
            {
                tactics.Add(tactic);
            }



            foreach (PirateTactics tactic in tactics)
            {
                distance = game.Distance(tactic.Pirate, tactic.FinalDestination);
                if (tactic.Pirate.HasTreasure)
                {
                    tactic.Moves = tactic.Pirate.CarryTreasureSpeed;
                }
                else if (movesleft == 0)
                {
                    tactic.Moves = 0;
                }
                else if (distance <= movesleft)
                {
                    tactic.Moves = distance;
                    movesleft -= distance;
                }
                else
                {
                    tactic.Moves = movesleft;
                    movesleft = 0;
                }

            }


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

        /// <summary>
        /// Returns list of enemy pirates that are sober and dont have treasure
        /// </summary>
        /// <param name="game"></param>
        /// <returns>Returns list of enemy pirates that are sober and dont have treasure</returns>
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

        //DO NOT TOUCH - ROEE FUNCITON
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

        //Oz: I'm going change the things that are connected to attack
        //roee divided TakeAction to 2 parts, this is the defence part.
        private void TakeActionDefence(IPirateGame game, PirateTactics tactics)
        {
            if (tactics != null)
            {
                if (tactics.Pirate.HasTreasure)
                {
                    foreach (Pirate enemyPirate in EnemyGoodPirates(game))
                    {
                        if (game.InRange(tactics.Pirate, enemyPirate) && enemyPirate.ReloadTurns == 0 && !enemyPirate.HasTreasure)
                        {
                            game.Defend(tactics.Pirate);
                            return;
                        }
                    }
                }
            }
        }
        //יש להוריד את החלק של ההגנה מפה 
        private void TakeAction(IPirateGame game, PirateTactics tactics)
        {
            if (tactics != null)
            {
                Pirate enemy = findEnemyWithTreasure(game, tactics.Pirate);
                if (tactics.Pirate.Powerups.Contains("attack"))
                {
                    enemy = minPirateFromPirates(game, tactics.Pirate, targetablePirates);
                }

                if (tactics.Pirate.HasTreasure || tactics.Pirate.Powerups.Contains("attack"))
                {
                    foreach (Pirate enemyPirate in EnemyGoodPirates(game))
                    {
                        if (game.InRange(tactics.Pirate, enemyPirate) && enemyPirate.ReloadTurns == 0 && !enemyPirate.HasTreasure && tactics.Pirate.DefenseReloadTurns == 0)
                        {
                            game.Defend(tactics.Pirate);
                            return;
                        }
                    }
                }
                if (enemy != null)
                {

                    if (game.InRange(tactics.Pirate, enemy) && tactics.Pirate.ReloadTurns == 0 && !tactics.Pirate.HasTreasure)
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

        /// <summary>
        /// Finds the closest treasure to the pirate
        /// </summary>
        /// <param name="game"></param>
        /// <param name="id"></param>
        /// <returns>Returns the closest treasure to the pirate</returns>
        private Treasure minTreasureFromPirate(IPirateGame game, Pirate pirate)
        {
            int distance;
            Treasure minTreasure = null;
            int minDistance = int.MaxValue;


            foreach (Treasure treasure in targetableTreasures)
            {
                distance = game.Distance(pirate, treasure) + game.Distance(pirate.InitialLocation, treasure.Location);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minTreasure = treasure;
                }
            }

            return minTreasure;

        }

        //tashtit
        private PirateTactics GetPirateTarget(IPirateGame game, Pirate pirate)
        {
            PirateTactics tactics = new PirateTactics() { Pirate = pirate };
            Pirate enemyPirate = minPirateFromPirates(game, pirate, targetablePirates);
            Treasure treasure = minTreasureFromPirate(game, pirate);
            Powerup powerup = TryPowerUp(game, pirate);

            if (tactics.Pirate.HasTreasure)
            {
                tactics.FinalDestination = tactics.Pirate.InitialLocation;
                tactics.destinationtarget = DestinationTarget.toInitial;
            }
            else if (pirate.Powerups.Contains("attack") && enemyPirate != null)
            {
                tactics.destinationtarget = DestinationTarget.toAttack;
                tactics.FinalDestination = enemyPirate.Location;

            }
            else if (pirate.Powerups.Contains("speed") && treasure != null)
            {
                tactics.destinationtarget = DestinationTarget.toTreasure;
                tactics.FinalDestination = treasure.Location;
            }
            else if (tryDefence(game, pirate, false))
            {
                tactics.destinationtarget = DestinationTarget.toDefence;
            }
            else if (powerup != null)
            {
                tactics.FinalDestination = powerup.Location;
                tactics.destinationtarget = DestinationTarget.toPowerUp;
            }
            else
            {
                enemyPirate = pirateinOurInitals(game);

                if (enemyPirate == null)
                {
                    enemyPirate = findEnemyWithTreasure(game, pirate);
                    
                    if(game.Treasures().Count > 1 && isBeginning(game))
                    {
                        if (treasure != null)
                        {
                            tactics.FinalDestination = treasure.Location;
                            targetableTreasures.Remove(treasure);
                            tactics.destinationtarget = DestinationTarget.toTreasure;
                        }
                    }

                    else if (enemyPirate != null && pirate.ReloadTurns == 0)
                    {
                        tactics.FinalDestination = enemyPirate.Location;
                        tactics.destinationtarget = DestinationTarget.toAttack;
                    }
                    else if (enemyPirate != null && (pirate.ReloadTurns != 0 || enemyPirate.DefenseExpirationTurns != 0))
                    {
                        List<Location> locations = game.GetSailOptions(enemyPirate, enemyPirate.InitialLocation, enemyPirate.CarryTreasureSpeed);
                        tactics.FinalDestination = locations[0];
                        tactics.destinationtarget = DestinationTarget.toCollide;
                        targetablePirates.Remove(enemyPirate);
                    }
                    else
                    {
                        if (treasure != null)
                        {
                            tactics.FinalDestination = treasure.Location;
                            targetableTreasures.Remove(treasure);
                            tactics.destinationtarget = DestinationTarget.toTreasure;
                        }
                        else
                        {
                            enemyPirate = minPirateFromPirates(game, pirate, targetablePirates);

                            if (enemyPirate != null && tactics.Pirate.Location != enemyPirate.InitialLocation && enemyPirate.ReloadTurns == 0)
                            {
                                tactics.FinalDestination = enemyPirate.InitialLocation;
                                tactics.destinationtarget = DestinationTarget.toCollide;
                                targetablePirates.Remove(enemyPirate);
                            }
                        }
                    }
                }
                else
                {

                    tactics.FinalDestination = enemyPirate.Location;
                    tactics.destinationtarget = DestinationTarget.toCollide;
                    targetablePirates.Remove(enemyPirate);
                }

            }

            tactics.Moves = PriorityGoldMoves(game, pirate, tactics.destinationtarget);

            return tactics;
        }

        private void NewPriority(IPirateGame game, List<PirateTactics> tactics)
        {
             
        }

        /// <summary>
        /// priority for the last gold ( because you want to pick up the last few golds).. will give the ones who are grabbing the gold 0 and those without gold max.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns></returns>
        private int PriorityGoldMoves(IPirateGame game, Pirate pirate, DestinationTarget destionationtarget)
        {
            if (pirate.TurnsToSober != 0)
            {
                return 0;
            }
            if (pirate.HasTreasure)
            {
                return pirate.CarryTreasureSpeed;
            }
            if (isMovesLeft && (destionationtarget != DestinationTarget.toCollide || !isPirateInPiratesInitial(game, pirate, game.EnemyPirates())))
            {
                isMovesLeft = false;
                return Maxmoves - SumOfAllTreasureMovements(game);
            }
            return 0;


        }

        //OZ: going to change it
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

        //Oz: going to change it
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

        public List<Treasure> GoodTreasures(IPirateGame game)
        {
            return null;
        }

        //functions added
        private void RemoveNulls(List<PirateTactics> pt)
        {
            int index = 0;
            while (index < pt.Count)
            {
                if (pt[index] == null)
                {
                    pt.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        private void NoCollision(IPirateGame game, List<PirateTactics> piratetactics)
        {
            List<Location> locations;

            foreach (PirateTactics tactic in piratetactics)
            {
                if (tactic.FinalDestination != null)
                {
                    locations = game.GetSailOptions(tactic.Pirate, tactic.FinalDestination, tactic.Moves);
                    NoCollisionOnlyDrunk(game, tactic, locations);
                    NoCollisionOnlyFriends(game, tactic, piratetactics, locations);
                    NoCollsionStanding(game, tactic, locations);
                    if (tactic.destinationtarget == DestinationTarget.toPowerUp)
                    {
                        NoCollisionCoins(game, tactic, locations);
                    }

                    game.Debug("-> pirate id : " + tactic.Pirate.Id);
                    game.Debug("-> final destination : " + tactic.FinalDestination);
                    game.Debug("-> temporary destination : " + tactic.TempDestination);
                    game.Debug("-> moves : " + tactic.Moves);
                    game.Debug("-> which target : " + tactic.whichTarget());
                    game.Debug("-------------------------");
                }

            }
        }

        private void NoCollisionOnlyFriends(IPirateGame game, PirateTactics tactic, List<PirateTactics> tactics, List<Location> locations)
        {


            foreach (PirateTactics tactic2 in tactics)
            {
                if (tactic.Pirate != tactic2.Pirate)
                {
                    locations.Remove(tactic2.TempDestination);
                }
            }
            if (locations.Count == 0 && tactic.Moves > 0)
            {
                tactic.Moves -= 1;
                locations = game.GetSailOptions(tactic.Pirate, tactic.FinalDestination, tactic.Moves);
                NoCollisionOnlyFriends(game, tactic, tactics, locations);
            }
            else if (locations.Count > 0 && (tactic.Moves == 0 && game.GetPirateOn(tactic.FinalDestination) != null) || tactic.Moves > 0)
            {
                tactic.TempDestination = locations[0];
            }
            else
            {
                tactic.Moves = -1;
                tactic.TempDestination = null;
            }
        }

        private void NoCollisionOnlyDrunk(IPirateGame game, PirateTactics tactic, List<Location> locations)
        {
            foreach (Pirate ePirate in game.EnemyDrunkPirates())
            {
                locations.Remove(ePirate.Location);
            }
            foreach (Pirate pirate in game.MyDrunkPirates())
            {
                locations.Remove(pirate.Location);
            }
            if (locations.Count == 0 && tactic.Moves > 0)
            {
                tactic.Moves -= 1;
                locations = game.GetSailOptions(tactic.Pirate, tactic.FinalDestination, tactic.Moves);
                NoCollisionOnlyDrunk(game, tactic, locations);
            }
            else if (tactic.Moves == 0 && game.GetPirateOn(tactic.FinalDestination) != null || tactic.Moves > 0)
            {
                tactic.TempDestination = locations[0];
            }
            else
            {
                tactic.Moves = -1;
                tactic.TempDestination = null;
            }
        }

        private void NoCollsionStanding(IPirateGame game, PirateTactics tactic, List<Location> locations)
        {
            foreach (Pirate pirate in game.EnemyPirates())
            {
                if (tactic.destinationtarget != DestinationTarget.toCollide)
                {
                    locations.Remove(pirate.Location);
                }
            }
            foreach (Pirate pirate in game.MyPirates())
            {
                locations.Remove(pirate.Location);

            }
            if (locations.Count == 0 && tactic.Moves > 0)
            {
                tactic.Moves -= 1;
                locations = game.GetSailOptions(tactic.Pirate, tactic.FinalDestination, tactic.Moves);
                NoCollisionOnlyDrunk(game, tactic, locations);
            }

            else if (locations.Count > 0 && tactic.Moves == 0 && game.GetPirateOn(tactic.FinalDestination) != null || tactic.Moves > 0)
            {
                tactic.TempDestination = locations[0];
            }

            else
            {
                tactic.Moves = -1;
                tactic.TempDestination = null;
            }

        }

        private void NoCollisionCoins(IPirateGame game, PirateTactics tactic, List<Location> locations)
        {
            foreach (Treasure treasure in game.Treasures())
            {
                locations.Remove(treasure.Location);

            }

            if (locations.Count == 0 && tactic.Moves > 0)
            {
                tactic.Moves -= 1;
                locations = game.GetSailOptions(tactic.Pirate, tactic.FinalDestination, tactic.Moves);
                NoCollisionOnlyDrunk(game, tactic, locations);
            }

            else if (locations.Count > 0 && tactic.Moves == 0 && game.GetPirateOn(tactic.FinalDestination) != null || tactic.Moves > 0)
            {
                tactic.TempDestination = locations[0];
            }

            else
            {
                tactic.Moves = -1;
                tactic.TempDestination = null;
            }


        }

        private Pirate minPirateFromPirates(IPirateGame game, Pirate pirate, List<Pirate> pirates)
        {
            int mindistance = int.MaxValue;
            Pirate minPirate = null;

            foreach (Pirate aPirate in pirates)
            {
                if (game.Distance(pirate, aPirate) < mindistance)
                {
                    mindistance = game.Distance(pirate, aPirate);
                    minPirate = aPirate;
                }
            }

            return minPirate;

        }

        private Pirate pirateinOurInitals(IPirateGame game)
        {

            Pirate ePirate;

            foreach (Pirate fPirate in game.MyPirates())
            {
                ePirate = game.GetPirateOn(fPirate.InitialLocation);
                if (ePirate != null && ePirate.Owner != 0 && targetablePirates.Contains(ePirate))
                {
                    return ePirate;
                }
            }

            return null;

        }

        private bool isPirateInPiratesInitial(IPirateGame game, Pirate pirate, List<Pirate> pirates)
        {
            foreach (Pirate aPirate in pirates)
            {
                if (game.GetPirateOn(aPirate.InitialLocation) == pirate)
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

        #region Not Used

        ////NOT FUNCTION UNTIL FIXED ADAIAN LO HISTAMASHNOO OZ AMAR
        ///// <summary>
        ///// calculating maxmoves that the pirate can do.(for having every boat to move).
        ///// </summary>
        ///// <param name="game"></param>
        ///// <param name="pirate"></param>
        ///// <returns>return a number between 0-maxmoves (depends on the api)</returns>
        //private int PriorityDistributionMoves(IPirateGame game, Pirate pirate)
        //{
        //    //checks for any treasures, if no treasure founds, just returns all the ships with treasures back to home.
        //    if (game.Treasures().Count > 0)
        //    {
        //        //if pirate has no treasure, doing mathematics for caluclaling that all ships will move in maximum effort.
        //        int _Maxmoves = 0;

        //        if (!pirate.HasTreasure)
        //        {
        //            int leftover = (Maxmoves - game.MyPiratesWithTreasures().Count) % (game.MyPiratesWithoutTreasures().Count - numOfAttackers(game));
        //            if (leftover != 0)
        //            {
        //                _Maxmoves = (Maxmoves - game.MyPiratesWithTreasures().Count) / (game.MyPiratesWithoutTreasures().Count - numOfAttackers(game));

        //                if (isMathworks)
        //                {
        //                    _Maxmoves += leftover;
        //                    //does this only one time a turn.
        //                    isMathworks = false;
        //                }
        //            }
        //            else
        //            {
        //                _Maxmoves = (Maxmoves - game.MyPiratesWithTreasures().Count) / (game.MyPiratesWithoutTreasures().Count - numOfAttackers(game));
        //            }
        //            return _Maxmoves;
        //        }
        //    }
        //    if (pirate.HasTreasure)
        //    {
        //        //its a trick!, if its -1 , ill know in the location, that it will need to go back to base.
        //        return -1;
        //    }
        //    //TODO: fix this
        //    return -1;

        //}

        ///// <summary>
        ///// retuns the number of attacking ships we control.
        ///// </summary>
        ///// <param name="game"></param>
        ///// <returns></returns>
        //private int numOfAttackers(IPirateGame game)
        //{
        //    int counter = 0;
        //    foreach (Pirate pirate in game.AllMyPirates())
        //    {
        //        if (attackMod && pirate.TurnsToSober == 0 && isWorthtoAttack(game, pirate, findTreasure(game, pirate)) != null)
        //        {
        //            counter++;
        //        }
        //    }
        //    return counter;
        //}

        ///// <summary>
        ///// gets the minimum location between set of locations and a pirate.
        ///// </summary>
        ///// <param name="game"></param>
        ///// <param name="locations"></param>
        ///// <param name="pirate"></param>
        ///// <returns></returns>
        ///*private Location minLocation(IPirateGame game ,List<Location> locations, Pirate pirate)
        //{

        //    Location minLocation = locations[0];
        //    int minDistance = game.Distance(pirate, minLocation);

        //    int distance;
        //    int id = 0;

        //    for (int i = 0; i < locations.Count; i++)
        //    {
        //        distance = game.Distance(pirate, locations[i]);
        //        if(distance < minDistance)
        //        {
        //            minDistance = distance;
        //            minLocation = locations[i];
        //            id = i;
        //        }
        //    }

        //    return minLocation;*/

        ///// <summary>
        ///// gets the pirate which its distance to the closest treature is the minimum
        ///// </summary>
        ///// <param name="game">the game</param>
        ///// <returns>the pirate with the characaristics we said</returns>
        //private Pirate minDistancePerStatus(IPirateGame game)
        //{
        //    if (MyGoodPirates(game).Count == 0)
        //    {
        //        return null;
        //    }
        //    Pirate minPirate = MyGoodPirates(game)[0];
        //    if (findTreasure(game, minPirate) == null)
        //    {
        //        return null;
        //    }
        //    int minDistance = game.Distance(minPirate, findTreasure(game, minPirate));
        //    foreach (Pirate pirate in MyGoodPirates(game))
        //    {
        //        if (minDistance > game.Distance(pirate, findTreasure(game, pirate)))
        //        {
        //            minDistance = game.Distance(pirate, findTreasure(game, pirate));
        //            minPirate = pirate;
        //        }
        //    }
        //    game.Debug("ITS OK " + minPirate.Id);
        //    return minPirate;
        //}

        ////going to delete it.
        ///// <summary>
        ///// find the closest treasure
        ///// </summary>
        ///// <param name="game"></param>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public Treasure findTreasure(IPirateGame game, Pirate pirate)
        ////TODO: name
        //{
        //    foreach (TakenTreasure tt in forbiddenTreasures)
        //    {
        //        //TODO: nameeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //        if (tt.pirate == pirate)
        //        {
        //            return tt.Treasure;
        //        }
        //    }

        //    List<Treasure> lst = game.Treasures();
        //    //TODO: name
        //    if (game.Treasures().Count == 0)
        //    {
        //        return null;
        //    }
        //    int minmovements = game.Distance(lst[0].Location, pirate.Location);
        //    Treasure minTreasure = lst[0];

        //    for (int i = 1; i < lst.Count; i++)
        //    {
        //        int mv = game.Distance(lst[i].Location, pirate.Location);
        //        if (mv < minmovements && !isForbiddenTreasure(game, lst[i]))
        //        {
        //            minmovements = mv;
        //            minTreasure = lst[i];
        //        }

        //    }

        //    forbiddenTreasures.Add(new TakenTreasure(pirate, minTreasure));
        //    return minTreasure;
        //}

        //private bool isForbiddenTreasure(IPirateGame game, Treasure treasure)
        //{
        //    foreach (TakenTreasure tr in forbiddenTreasures)
        //    {
        //        if (tr.Treasure == treasure)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private Pirate CloseToFriendPirate(IPirateGame game, Pirate pirate)
        //{
        //    int distance;
        //    Pirate minLocationPirate = null;
        //    List<Pirate> closeToBasePirates = new List<Pirate>();
        //    int checkeddistance = game.Distance(pirate, pirate.InitialLocation);


        //    foreach (Pirate friendPirate in MyGoodPirates(game))
        //    {
        //        distance = game.Distance(friendPirate, pirate.InitialLocation);

        //        if (distance < checkeddistance)
        //        {
        //            closeToBasePirates.Add(friendPirate);
        //        }

        //    }

        //    distance = int.MaxValue;

        //    foreach (Pirate closePirate in closeToBasePirates)
        //    {
        //        if (game.Distance(closePirate, pirate) < distance)
        //        {
        //            distance = game.Distance(closePirate, pirate);
        //            minLocationPirate = closePirate;
        //        }
        //    }

        //    return minLocationPirate;
        //}

        //public Treasure FarFromEnemiesTrs(IPirateGame game, Pirate pirate)
        //{
        //    int sumofDistance;
        //    List<int> treasureDistances = new List<int>();
        //    List<Treasure> SortedTreasuresByDisFromEnemies = new List<Treasure>();


        //    foreach (Treasure treasure in game.Treasures())
        //    {
        //        sumofDistance = 0;

        //        foreach (Pirate ePirate in game.EnemySoberPirates())
        //        {
        //            sumofDistance += game.Distance(ePirate, treasure);
        //        }
        //        treasureDistances.Add(sumofDistance);

        //    }

        //    return null;
        //}

        ////is going to be removed
        ///// <summary>
        ///// target a pirate location, and return the pirates tactic, require the game, the status of the game, and the id of the pirate to target .
        ///// </summary>
        ///// <param name="game">The game</param>
        ///// <param name="status">status of the game</param>
        ///// <param name="id">id of the pirate</param>
        ///// <returns></returns>
        //private PirateTactics GetPirateTarget(IPirateGame game, Pirate pirate)
        //{

        //    PirateTactics tactics = new PirateTactics() { Pirate = pirate };

        //    tactics.Moves = PriorityGoldMoves(game, pirate);

        //    //TODO : is going to be moved
        //    game.Debug("-> pirate id : " + tactics.Pirate.Id);
        //    game.Debug("-> destination : " + tactics.FinalDestination);
        //    game.Debug("-> moves : " + tactics.Moves);
        //    game.Debug("-------------------------");


        //    if (tactics.Moves != 0)
        //    {
        //        //for -1 reference, see distribtionGold
        //        //TODO: not very good if's
        //        if (tactics.Moves == -1)
        //        {
        //            tactics.FinalDestination = tactics.Pirate.InitialLocation;
        //            tactics.Moves = 1;/**/
        //        }
        //        else
        //        {
        //            Pirate enemyPirate = findEnemyWithTreasure(game, pirate);
        //            if (enemyPirate != null && pirate.ReloadTurns == 0)
        //            {
        //                tactics.FinalDestination = enemyPirate.Location;
        //                game.Debug("happend!!");

        //            }
        //            else
        //            {
        //                tactics.FinalDestination = minTreasureFromPirate(game, pirate).Location;
        //            }
        //        }

        //        //List<Location> possibleLocations = game.GetSailOptions(tactics.Pirate,
        //        //tactics.FinalDestination, tactics.Moves);
        //        //tactics.TempDestination = possibleLocations[0];

        //        if (tactics.Moves != 0)
        //        {
        //            return tactics;
        //        }
        //        return null;
        //    }
        //    //Returns null when the pirate doesn't move.
        //    return null;
        //}
        #endregion

        //Don't touch: attack functions
        //OZ: going to add some helpful attack functions

        #region Helpful Attack Functions

        #region formula
        // The formula isn't working with the new powerups (his speed may be more than 1)       
        private int LastChanceMinAttack(IPirateGame game, Pirate friendlyPirate, Pirate enemyPirate)
        {
            int y = game.Distance(friendlyPirate, enemyPirate);
            int z = game.Distance(enemyPirate, enemyPirate.InitialLocation);
            double solution = (double)(y + z) / y;

            if ((int)solution < solution)
            {
                solution++;
            }
            return (int)solution;
        }
        #endregion

        /// <summary>
        /// Return true if we lose
        /// </summary>
        /// <param name="game"></param>
        /// <returns>Returns true if we lose else false</returns>
        private bool IsLose(IPirateGame game)
        {
            return ((game.GetMyScore() + game.MyPiratesWithTreasures().Count) <
                (game.GetEnemyScore() + game.EnemyPiratesWithTreasures().Count));
        }

        /// <summary>
        /// Finds the closest sober enemy
        /// </summary>
        /// <param name="game">This Game</param>
        /// <param name="pirate">My pirate</param>
        /// <returns>The closest sober enemy</returns>
        private Pirate GetClosestEnemy(IPirateGame game, Pirate pirate)
        {
            //checks if there are Sober Enemy pirates
            if (game.EnemySoberPirates().Count > 0)
            {
                //The closest enemy
                Pirate closenemy = game.EnemySoberPirates()[0];

                //running on EnemySoberPirates List
                foreach (Pirate enemy in game.EnemySoberPirates())
                {
                    //EQUALLS the Distances between My pirate to the Enemy pirates 
                    if (game.Distance(pirate, enemy) < game.Distance(pirate, closenemy))
                    {
                        //Chooses the Closer Enemy pirate
                        closenemy = enemy;
                    }
                }

                //returns the Closest Enemy pirate
                return closenemy;
            }

            //returns NULL if there is NO Sober Enemies on the Map
            return null;
        }

        /// <summary>
        /// Checks which enemies are in my Radius of attack
        /// </summary>
        /// <param name="game">This Game</param>
        /// <param name="pirate">My pirate</param>
        /// <returns>List of sober enemy pirates that are in my radius</returns>
        private List<Pirate> SoberEnemiesInRadius(IPirateGame game, Pirate pirate)
        {
            //List that contains all the enemy pirates that are sober
            List<Pirate> radEnemies = new List<Pirate>();

            //running on EnemySoberPirates List
            foreach (Pirate enemy in game.EnemySoberPirates())
            {
                //checks if the enemy is in Attack Radius
                if (game.InRange(pirate, enemy))
                {
                    //if he is in my Radius than add him to the List
                    radEnemies.Add(enemy);
                }
            }

            //Returns the list of sober enemies that are in my radius of attack
            return radEnemies;
        }

        /// <summary>
        /// Checks the game state
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns>Returns true if the game state is beginning else false</returns>
        private bool isBeginning(IPirateGame game)
        {
            bool beginning = true;
            int totalTreasures = TotalTreasuresInGame(game);
            int treasuresToWin = totalTreasures / 2 + 1;
            int takenTreasures = totalTreasures - game.Treasures().Count;

            if (takenTreasures >= treasuresToWin / 2)
            {
                beginning = false;
            }

            return beginning;
        }

        /// <summary>
        /// Checks if there is sober enemy (without treasure) that is loaded and closer than the treasure
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns></returns Returns loaded sober enemy (without treasure) that is closer than the treasure>
        private Pirate IsEnemyCloseAndLoaded(IPirateGame game, Pirate pirate)
        {
            // מחזירה פיראט אויב טעון בלי אוצר שהמרחק שלך אליו קטן מהמרחק שלך לאוצר
            foreach (Pirate enemyPirate in EnemyGoodPirates(game))
            {
                if (game.Distance(enemyPirate, pirate) < game.Distance(pirate, minTreasureFromPirate(game, pirate)) && enemyPirate.ReloadTurns == 0)
                {
                    return enemyPirate;
                }
            }
            // אם אין כאלה פיראטים מחזירה null
            return null;
        }

        /// <summary>
        /// Calculated the number of total treasures in game
        /// </summary>
        /// <param name="game"></param>
        /// <returns>Returns the number of total treasures in game</returns>
        private int TotalTreasuresInGame(IPirateGame game)
        {
            int sum = game.Treasures().Count + game.MyPiratesWithTreasures().Count +
            game.EnemyPiratesWithTreasures().Count + game.GetMyScore() + game.GetEnemyScore();

            return sum;
        }

        /// <summary>
        /// returns sorted by distance List of PirateTactics
        /// </summary>
        /// <param name="game"></param>
        /// <param name="tactics"></param>
        /// <returns>Returns sorted by distance List of PirateTactics</returns>
        private List<PirateTactics> SortTacticsByDistance(IPirateGame game, List<PirateTactics> tactics)
        {

            List<PirateTactics> sortedTactics = new List<PirateTactics>();

            // אם הרשימה ריקה החזר רשימה ריקה
            if (tactics.Count == 0)
            {
                return sortedTactics;
            }

            PirateTactics shortTactic = tactics[0];
            while (tactics.Count > 0)
            {
                foreach (PirateTactics tactic in tactics)
                {
                    if (game.Distance(tactic.Pirate, tactic.FinalDestination) <
                        game.Distance(shortTactic.Pirate, shortTactic.FinalDestination))
                    {
                        shortTactic = tactic;
                    }
                }
                sortedTactics.Add(shortTactic);
                tactics.Remove(shortTactic);
            }

            return sortedTactics;
        }

        #endregion

        #region Always

        /// <summary>
        /// Checks if there is a sober enemy with treasure that is closer than the treasure
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns>Returns the closest enemy enemy with treasure that is closer than the treasure </returns>
        private Pirate ThiefTactic(IPirateGame game, Pirate pirate)
        {

            Pirate minEnemyPirate = null;
            int minDistance = int.MaxValue;

            foreach (Pirate enemyPirate in game.EnemyPiratesWithTreasures())
            {
                int distanceToEnemy = game.Distance(enemyPirate, pirate);
                if (distanceToEnemy < game.Distance(pirate, minTreasureFromPirate(game, pirate))
                    && distanceToEnemy < minDistance)
                {
                    minDistance = distanceToEnemy;
                    minEnemyPirate = enemyPirate;
                }
            }
            if (minEnemyPirate != null)
            {
                return minEnemyPirate;
            }

            return null;

        }

        #endregion

        #region Start Game

        /// <summary>
        /// Checks if there is an enemy that is going to get my treasure before me
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns>The closest enemy that is going to get my teasure before me</returns>
        //TODO: Priority if there are 2 Pirates
        private Pirate PreventTactic(IPirateGame game, Pirate pirate)
        {
            Pirate minEnemyPirate = null;
            int minDistance = int.MaxValue;
            Treasure myTreasure = minTreasureFromPirate(game, pirate);
            foreach (Pirate enemyPirate in EnemyGoodPirates(game))
            {
                int enemyDistanceFromTreasure = game.Distance(enemyPirate, myTreasure);

                if (enemyDistanceFromTreasure < game.Distance(pirate, myTreasure) && enemyDistanceFromTreasure < minDistance)
                {
                    minEnemyPirate = enemyPirate;
                    minDistance = enemyDistanceFromTreasure;
                }
            }

            if (minEnemyPirate != null)
            {
                return minEnemyPirate;
            }

            return null;
        }


        #endregion

        #region End Game

        /// <summary>
        /// If we lose returns the closest enemy pirate with treasure else null
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <returns>Returns the closest enemy pirate with treasure if we lose, else null</returns>
        private Pirate LastChance(IPirateGame game, Pirate pirate)
        {
            if (IsLose(game) && game.EnemyPiratesWithTreasures().Count > 0)
            {
                int minDistance = int.MaxValue;
                Pirate closestEnemyPirate = null;

                foreach (Pirate enemyPirate in game.EnemyPiratesWithTreasures())
                {
                    int distance = game.Distance(enemyPirate, pirate);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestEnemyPirate = enemyPirate;
                    }

                }
                return closestEnemyPirate;
            }

            return null;
        }

        #endregion


        //TODO: Roee i need this function

        /// <summary>
        /// its the same as your funcion but i need bool(once for pirate target , and i dont want to defend, and once for takeaction)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate"></param>
        /// <param name="toDefend"></param>
        /// <returns></returns>
        private bool tryDefence(IPirateGame game, Pirate pirate, bool toDefend)
        {
            return false;
        }

        private int SumOfAllTreasureMovements(IPirateGame game)
        {
            int moves = 0;

            foreach (Pirate pirate in game.MyPiratesWithTreasures())
            {
                moves += pirate.CarryTreasureSpeed;
            }
            return moves;
        }

        private Powerup TryPowerUp(IPirateGame game, Pirate pirate)
        {
            if (game.Powerups().Count > 0)
            {
                Powerup closePower = game.Powerups()[0];
                foreach (Powerup power in game.Powerups())
                {
                    if (game.Distance(pirate, power.Location) < game.Distance(pirate, closePower.Location))
                        closePower = power;
                }
                return closePower;
            }
            return null;
        }

        private int PriorityDistributionMoves(IPirateGame game, Pirate pirate)
        {

            int _Maxmoves = 0;

            if (pirate.HasTreasure)
            {
                return pirate.CarryTreasureSpeed;
            }
            if(pirate.TurnsToSober != 0)
            {
                return 0;
            }
            if (!pirate.HasTreasure)
            {
                int leftover = (Maxmoves - SumOfAllTreasureMovements(game)) % (MyGoodPirates(game).Count);
                if (leftover != 0)
                {
                    _Maxmoves = (Maxmoves - SumOfAllTreasureMovements(game)) / (MyGoodPirates(game).Count);

                    if (isMathworks)
                    {
                        _Maxmoves += leftover;
                        //does this only one time a turn.
                        isMathworks = false;
                    }
                }
                else
                {
                    _Maxmoves = (Maxmoves - SumOfAllTreasureMovements(game)) / (MyGoodPirates(game).Count);
                }
                return _Maxmoves;
            }


            //TODO: fix this
            return 0;
        }

        private void SortByDistance(IPirateGame game, List<PirateTactics> tactics)
        {
            List<PirateTactics> sortedList = new List<PirateTactics>();
            int mindistance = int.MaxValue;

            
        }
        
    }
}
