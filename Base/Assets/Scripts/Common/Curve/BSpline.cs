using UnityEngine;
using System.Collections;

public class BSpline 
{
    public const int BSP_UNIFORM = 0;
	public const int BSP_CLAMPED = 1;
	
	public int curveType;
	public float[] knotVector; // knot vector
	public float[,] controlPoint; // n-dim control point
	public int numControlPoint; // # of control point
	public int order; // # of order (= degree + 1)
	public int dim; // # of dimension (x, y, z, ...)
	
	
	public BSpline(int _dim, int _order, int _numControlPoint, int _curveType)
	{
		// check that the B-Spline curve is valid
		if (_numControlPoint < _order)
			return;
		
		dim = _dim;
		order = _order;
		numControlPoint = _numControlPoint;
		curveType = _curveType;
		controlPoint = new float[numControlPoint, dim];
		knotVector = new float[numControlPoint + order];
		
		float step = 1.0f / (numControlPoint - order + 1);
		;
		for (int i = 0; i < numControlPoint + order; i++)
			knotVector[i] = (i - order + 1) * step;
		
		if (curveType == BSP_CLAMPED)
		{
			for (int i = 0; i < order; i++)
			{
				knotVector[i] = 0.0f;
				knotVector[numControlPoint + order - 1 - i] = 1.0f;
			}
		}
	}
	
	public int SearchRange(float _param, int _startIndex, int _endIndex)
	{
		if (_endIndex - _startIndex > 1)
		{
			int middleIndex = (_startIndex + _endIndex) / 2;
			if (_param < knotVector[middleIndex])
				return SearchRange(_param, _startIndex, middleIndex);
			else
				return SearchRange(_param, middleIndex, _endIndex);
		} else
			return _startIndex;
	}
	
	public void SetControlPointArray(int _dim, float[] _point)
	{
		for (int i = 0; i < numControlPoint; i++)
        {
            controlPoint[i, _dim] = _point[i];
        }
	}
	
	public void SetControlPoint(int _idx, float[] _point)
	{
		for (int i = 0; i < dim; i++)
			controlPoint[_idx, i] = _point[i];
	}
	
	public void GetControlPoint(int _idx, float[] _point)
	{
		for (int i = 0; i < dim; i++)
			_point[i] = controlPoint[_idx, i];
	}
	
	public void Evaluate(float _param, float[] _outPoint)
	{
		for (int i = 0; i < dim; i++)
			_outPoint[i] = 0.0f;
		int startControlPoint = SearchRange(_param, order - 1, numControlPoint)
				- order + 1;
		for (int i = startControlPoint; i < startControlPoint + order; i++)
		{
			float w = BasisFunc(_param, i, order);
			for (int j = 0; j < dim; j++)
				_outPoint[j] += w * controlPoint[i, j];
		}
	}
	
	public float BasisFunc(float param, int idx, int order)
	{
		// Cox-de Boor algorithm
		if (order > 1)
		{
			float b1 = BasisFunc(param, idx, order - 1);
			float b2 = BasisFunc(param, idx + 1, order - 1);
			float w1 = 0.0f, w2 = 0.0f;
			if (b1 > 0.0f)
				w1 = (param - knotVector[idx])
						/ (knotVector[idx + order - 1] - knotVector[idx]);
			if (b2 > 0.0f)
				w2 = (knotVector[idx + order] - param)
						/ (knotVector[idx + order] - knotVector[idx + 1]);
			return b1 * w1 + b2 * w2;
		} else
		{
			if (param >= knotVector[idx] && param < knotVector[idx + 1])
				return 1.0f;
			else if (param == knotVector[idx + 1] && idx + 1 == numControlPoint
					&& curveType == BSP_CLAMPED)
				return 1.0f;
			else
				return 0.0f;
		}
	}
}
