using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineBreeder : MonoBehaviour
{

    int crossSuc;
    Vector3 midOffset = Vector3.zero;
    List<Vector2> mirroredIO;

    public IEnumerator BreedMachines(List<GameObject> bestParents, List<GameObject> emptyMachines)
    {
        int emptyCount = 0;

        Debug.Log("possible crossovers: " + emptyMachines.Count);
        crossSuc = 0;

        for (int i = 0; i < bestParents.Count; i += 2)
        {
            OnePointCrossover(bestParents[i], bestParents[i + 1], emptyMachines[emptyCount]);
            emptyCount++;
        }

        bestParents.Clear();
        emptyMachines.Clear();

        Debug.Log("successfull crossovers: " + crossSuc);

        yield return null;
    }

    private void OnePointCrossover(GameObject parent1, GameObject parent2, GameObject empty)
    {
        List<GameObject> p1 = parent1.GetComponent<Machine>().Segments;
        List<GameObject> p2 = parent2.GetComponent<Machine>().Segments;

        //calculate middle index for both machines
        int midP1 = (p1.Count / 2) - 1;
        int midP2 = p2.Count / 2;

        //disable latter half of fisrt machine
        for (int i = midP1 + 1; i < p1.Count; i++)
        {
            p1[i].SetActive(false);
        }

        //calculate offset and machine parts
        Vector2 offset = p1[midP1].GetComponent<SegmentPart>().Output - p2[midP2].GetComponent<SegmentPart>().Input;
        List<GameObject> combined = p1.GetRange(0, midP1 + 1);
        combined.AddRange(p2.GetRange(midP2, p2.Count - midP2));

        //Debug.Log("Trying crossover with: " + parent1.name + " and: " + parent2.name);
        //Debug.Log("Parent1 at: " + midP1 + " , Parent2 at: " + midP2);

        if(p1[midP1].GetComponent<SegmentPart>().OutputDirection.x == p2[midP2].GetComponent<SegmentPart>().InputDirection.x)
        {
            //Debug.Log("Crossover point has matching segment directions");
            if (CheckCollision(p2.GetRange(midP2, p2.Count - midP2), offset)
                && CheckAreaFit(combined, offset, midP1, parent1.transform.position, false))
            {
                crossSuc++;
                DeleteEmpty(empty);
                CombineMachinePartsIntoEmpty(midP1, midP2, parent1, parent2, empty, false);
                empty.name += "X";
            }
        } else
        {
            //Debug.Log("Crossover point does not have matching segment directions");
            if (CheckCollisionMirrored(p2.GetRange(midP2, p2.Count - midP2), offset)
                && CheckAreaFit(combined, offset, midP1, parent1.transform.position, true))
            {
                crossSuc++;
                DeleteEmpty(empty);
                CombineMachinePartsIntoEmpty(midP1, midP2, parent1, parent2, empty, true);
                empty.name += "Y";
            }
        }
        
        for (int i = midP1; i < p1.Count; i++)
        {
            p1[i].SetActive(true);
        }
    }

    private bool CheckAreaFit(List<GameObject> combinedMachine, Vector2 offset, int mid, Vector2 middle, bool mirrored)
    {
        float minX = float.PositiveInfinity, minY = float.PositiveInfinity, maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;

        for (int i = 0; i < combinedMachine.Count; i++)
        {
            //might need to use boundingbox extremes instead
            Vector2 input = combinedMachine[i].GetComponent<SegmentPart>().Input;
            Vector2 output = combinedMachine[i].GetComponent<SegmentPart>().Output;

            if (i > mid)
            {
                if (mirrored)
                {
                    int index = (i - mid - 1) * 2;
                    input = mirroredIO[index];
                    output = mirroredIO[index+1];
                } else
                {
                    input += offset;
                    output += offset;
                }
            }

            if (input.x < minX) minX = input.x;
            if (input.x > maxX) maxX = input.x;
            if (input.y < minY) minY = input.y;
            if (input.y > maxY) maxY = input.y;

            if (output.x < minX) minX = output.x;
            if (output.x > maxX) maxX = output.x;
            if (output.y < minY) minY = output.y;
            if (output.y > maxY) maxY = output.y;
        }

        //4 corners of machine area rectangle + buffer
        minX -= 4f; minY -= 3f; maxX += 4f; maxY += 3f;
        float areaSize = SettingsReader.Instance.MachineSettings.MachineArea;

        //RGMTest.DrawRectangle(new Vector2(maxX, maxY), new Vector2(minX, minY), Color.magenta, 10);

        //if area is smaller than restriction, find center offset and return true, else false
        if ((maxX - minX) < areaSize && (maxY - minY) < areaSize)
        {
            Vector2 machineMiddle = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
            //Debug.Log(minX + "," + minY + " : " + maxX + "," + maxY);
            //Debug.Log("machmid: " + machineMiddle + " mid: " + middle);
            midOffset = middle - machineMiddle;
            return true;
        }
        //Debug.Log("does not fit");
        return false;
    }

    private bool CheckCollision(List<GameObject> machine, Vector2 offset)
    {
        foreach (GameObject seg in machine)
        {
            //check for any collision but restriction area
            if (!seg.GetComponent<SegmentLogic>().CheckSegmentOverlap(offset, "restriction area", false, false, 0))
            {
                //Debug.Log(seg.transform.parent.name + " collision: " + seg.name);
                return false;
            }
        }
        return true;
    }

    private bool CheckCollisionMirrored(List<GameObject> machine, Vector2 offset)
    {
        mirroredIO = new List<Vector2>();

        Vector2 newOffset = offset;
        Vector2 prevOutput = Vector2.zero;

        for (int i = 0; i < machine.Count; i++)
        {
            if (i > 0)
            {
                //calculate new offset: output - input where output is output of last iteration
                newOffset = prevOutput - machine[i].GetComponent<SegmentPart>().Input;
            }
            //calculate mirrored output location, newOutput = input + offset + (output - input) * (-1,1)
            prevOutput = machine[i].GetComponent<SegmentPart>().Input + newOffset + (machine[i].GetComponent<SegmentPart>().Output - machine[i].GetComponent<SegmentPart>().Input) * new Vector2(-1, 1);

            //save new offset, input and output for area fit calc
            mirroredIO.Add(machine[i].GetComponent<SegmentPart>().Input + newOffset);
            mirroredIO.Add(prevOutput);

            //check for any collision
            if (!machine[i].GetComponent<SegmentLogic>().CheckSegmentOverlap(newOffset, "restriction area", false, true, 0))
            {
                //Debug.Log(machine[i].transform.parent.name + " collision: " + machine[i].name);
                return false;
            }
        }
        return true;
    }

    private void DeleteEmpty(GameObject empty)
    {
        Machine emptyMachine = empty.GetComponent<Machine>();

        Destroy(emptyMachine.AutoStart);
        foreach (GameObject seg in emptyMachine.Segments)
        {
            Destroy(seg);
        }
        emptyMachine.Segments.Clear();

        emptyMachine.Canvas.transform.GetChild(0).GetComponent<BarChart>().ResetBars();
        emptyMachine.Fitness = 0;
    }

    private void CombineMachinePartsIntoEmpty(int midP1, int midP2, GameObject parent1, GameObject parent2, GameObject empty, bool mirror)
    {
        GameObject copy;

        List<GameObject> parentMachine1 = parent1.GetComponent<Machine>().Segments;
        List<GameObject> parentMachine2 = parent2.GetComponent<Machine>().Segments;

        Vector3 offset1 = empty.transform.position - parent1.transform.position;
        Vector3 offset2 = (empty.transform.position + ((Vector3)parentMachine1[midP1].GetComponent<SegmentPart>().Output - parent1.transform.position)) - (Vector3)parentMachine2[midP2].GetComponent<SegmentPart>().Input;

        //center machines
        offset1 += midOffset;
        offset2 += midOffset;

        //copy auto start of first parent into empty machine
        copy = Instantiate(parent1.GetComponent<Machine>().AutoStart, (parent1.GetComponent<Machine>().AutoStart.transform.position + offset1), Quaternion.identity);
        copy.transform.parent = empty.transform;
        empty.GetComponent<Machine>().AutoStart = copy;
        copy.GetComponent<AutoStart>().PistonDirection = parent1.GetComponent<Machine>().AutoStart.GetComponent<AutoStart>().PistonDirection;

        //copy first parents segments up until index, then second parents segments
        for (int i = 0; i <= midP1; i++)
        {
            CopySegment(parentMachine1[i].gameObject, offset1, empty, false); 
        }

        for (int i = midP2; i < parentMachine2.Count; i++)
        {
            if(i > midP2 && mirror)
            {
                offset2 += (Vector3)((parentMachine2[i - 1].GetComponent<SegmentPart>().Output - parentMachine2[i - 1].GetComponent<SegmentPart>().Input) * new Vector2(-1,1));
                offset2 = ((Vector3)parentMachine2[i - 1].GetComponent<SegmentPart>().Input + offset2) - (Vector3)parentMachine2[i].GetComponent<SegmentPart>().Input;
            }

            CopySegment(parentMachine2[i].gameObject, offset2, empty, mirror);
        }
    }

    public void CopySegment(GameObject seg, Vector3 offset, GameObject empty, bool mirror)
    {
        GameObject copy;

        //copy segments 
        copy = Instantiate(seg, (seg.transform.position + offset), seg.transform.rotation);

        copy.name = seg.name;
        copy.GetComponent<SegmentLogic>().GetDataReference();
        copy.GetComponent<SegmentPart>().CopyProperties(copy, seg, offset);
        copy.GetComponent<SegmentPart>().SegmentID = seg.GetComponent<SegmentPart>().SegmentID;

        if (mirror)
        {
            copy.GetComponent<SegmentPart>().MirrorSegment();
        }

        copy.transform.parent = empty.transform;
        empty.GetComponent<Machine>().Segments.Add(copy);
    }
}
