using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MachineSelector : MonoBehaviour
{
    //returns list of parent pairs in descending order
    public IEnumerator SelectMachines(List<GameObject> population, List<GameObject> bestParents, bool feasible, bool crowding)
    {
        if(population.Count < 2)
        {
            Debug.Log("population is too small to select parents");
            yield break;
        }
        
        //thee forths of populationsize will be chosen as parents
        int numParents = Mathf.RoundToInt(SettingsReader.Instance.EASettings.ParentSize * population.Count);
        if(numParents % 2 != 0)
        {
            numParents++;
        }

        if (feasible)
        {
            //random selection, pass copy of population as it will be changed
            if(SettingsReader.Instance.EASettings.NSGA == 2)
            {
                TournamentSelectionNSGA2(new List<GameObject>(population), bestParents, numParents, 2, crowding);
            } else
            {
                RandomSelection(new List<GameObject>(population), bestParents, numParents);
            }
        } else
        {
            //Tournament selection
            TournamentSelection(new List<GameObject>(population), bestParents, numParents, 2);
        }

        yield return null;
    }

    private void RandomSelection(List<GameObject> population, List<GameObject> bestParents, int numParents)
    {
        //random index
        int randomPick = 0;

        //randomly pick parents and add to best parents
        for (int i = 0; i < numParents; i++)
        {
            randomPick = Random.Range(0, population.Count);
            bestParents.Add(population[randomPick]);
            population.RemoveAt(randomPick);
        }
    }

    //tournament selection without replacement
    private void TournamentSelection(List<GameObject> population, List<GameObject> bestParents, int numParents, int TournamentSize)
    {
        int randomPick = 0;

        int feasIndex = SettingsReader.Instance.EASettings.FitFunc.Count - 1;

        GameObject winner = null;

        List<GameObject> tournament;

        for (int i = 0; i < numParents; i++)
        {
            //new tournament for each parent to choose
            tournament = new List<GameObject>();
            //choose random individuals from population for tournament with set tournamentsize
            for (int j = 0; j < TournamentSize; j++)
            {
                randomPick = Random.Range(0, population.Count);
                tournament.Add(population[randomPick]);
            }
            //find best in tournament based on feasibility
            winner = tournament[0];
            for (int j = 1; j < TournamentSize; j++)
            {
                if(winner.GetComponent<Machine>().FitnessVals[feasIndex] < tournament[j].GetComponent<Machine>().FitnessVals[feasIndex])
                {
                    winner = tournament[j];
                }
            }

            //add winner to best parents and remove from copied population
            bestParents.Add(winner);
            population.Remove(winner);
        }
    }

    //tournament selection without replacement and crowding comparison
    private void TournamentSelectionNSGA2(List<GameObject> population, List<GameObject> bestParents, int numParents, int TournamentSize, bool crowding)
    {
        int randomPick = 0;

        GameObject winner = null;

        List<GameObject> tournament;

        for (int i = 0; i < numParents; i++)
        {
            //new tournament for each parent to choose
            tournament = new List<GameObject>();
            //choose random individuals from population for tournament with set tournamentsize
            for (int j = 0; j < TournamentSize; j++)
            {
                randomPick = Random.Range(0, population.Count);
                tournament.Add(population[randomPick]);
            }
            //find best in tournament based on feasibility
            winner = tournament[0];
            for (int j = 1; j < TournamentSize; j++)
            {
                if (winner.GetComponent<Machine>().NonDominationRank > tournament[j].GetComponent<Machine>().NonDominationRank)
                {
                    winner = tournament[j];
                } else if(winner.GetComponent<Machine>().NonDominationRank == tournament[j].GetComponent<Machine>().NonDominationRank)
                {
                    if (crowding)
                    {
                        if(winner.GetComponent<Machine>().CrowdingDistance < tournament[j].GetComponent<Machine>().CrowdingDistance)
                        {
                            winner = tournament[j];
                        }
                    }
                }
            }

            //add winner to best parents and remove from copied population
            bestParents.Add(winner);
            population.Remove(winner);
        }
    }

    /* NO EVOLUTION METHODS ONLY AUTO
    IEnumerator ManualSelection(List<GameObject> population, int numPairs)
    {
        //enable selection box and subscribe selection event
        foreach (GameObject machine in population)
        {
            machine.GetComponent<BoxCollider2D>().enabled = true;
            machine.GetComponent<Machine>().MachineSelectedEvent += OnMachineSelected;
        }

        //wait until n parents are selected
        while (parents.Count < numPairs)
        {
            yield return null;
        }

        //unsubscribe events and disable selection box
        foreach (GameObject machine in population)
        {
            machine.GetComponent<Machine>().MachineSelectedEvent -= OnMachineSelected;
            machine.GetComponent<BoxCollider2D>().enabled = false;
            //destroy all machines that were not selected
            if (!machine.GetComponent<Machine>().IsSelected)
            {
                empty.Add(machine);
            }
            else
            {
                machine.GetComponent<Machine>().SwitchSelect();
            }
        }

        //delete one machine for each pair of best parents, with worst overall fitness
        if (empty.Count > numPairs / 2)
        {
            List<GameObject> sortedEmpty = empty.OrderByDescending(x => x.GetComponent<Machine>().Fitness).ToList();
            for (int i = sortedEmpty.Count - 1; empty.Count > numPairs / 2; i--)
            {
                empty.Remove(sortedEmpty[i]);
            }
        }

    }

    IEnumerator AutoSelection(List<GameObject> population, int numPairs)
    {
        //save n / 3 pairs of best parents 
        for(int i = 0; i < population.Count; i++)
        {
            if (i < numPairs)
            {
                parents.Add(population[i]);
            }
        }

        //delete one machine for each pair of best parents, with worst overall fitness
        for(int i = population.Count-1; empty.Count < numPairs/2; i--)
        {
            empty.Add(population[i]);
        }

        yield return null;
    }

    //add / remove from parents if selected
    public void OnMachineSelected(GameObject machine)
    {
        if (parents.Contains(machine))
        {
            parents.Remove(machine);
        }
        else
        {
            parents.Add(machine);
        }
    }


    //int evolutionMethod = SettingsReader.Instance.EASettings.EvolutionMethod;
    if(evolutionMethod == 1)
    {
        //auto
        cur = new Task(AutoSelection(population, numPairs));
        while (cur.Running) yield return null;
    } else if(evolutionMethod == 2)
    {
        //manual
        cur = new Task(ManualSelection(population, numPairs));
        while (cur.Running) yield return null;
    } else
    {
        //interactive
    }
    */

}
