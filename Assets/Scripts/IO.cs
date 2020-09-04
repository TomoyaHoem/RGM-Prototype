using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Input/Output object for segments, stores various information about in- and output such as direction, location, etc.
public class IO
{

    //location of in- and output
    private Vector2 _input;
    private Vector2 _output;

    public Vector2 Input
    {
        get
        {
            return _input;
        }
        set
        {

        }
    }

    public Vector2 Output
    {
        get
        {
            return _output;
        }
        set
        {

        }
    }

    //direction of in- and output
    public Vector2 Direction()
    {
            return _output-_input;
    }

    public IO(Vector2 input, Vector2 output)
    {
        _input = input;
        _output = output;
    }

}
