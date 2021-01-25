using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MachineSelector : MonoBehaviour
{

    List<GameObject> parents;
    List<GameObject> empty;

    Task cur;

    public IEnumerator SelectMachines(List<GameObject> population, List<GameObject> bestParents, List<GameObject> emptyMachines)
    {
        parents = bestParents;
        empty = emptyMachines;

        int numPairs = (population.Count / 3) * 2;
        int evolutionMethod = SettingsReader.Instance.EASettings.EvolutionMethod;

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
    }

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

}
