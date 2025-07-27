void CirclePattern_float(float2 uv, float2 center, float radius, float smooth, out float output)
{
    float circle = pow((uv.y - center.y), 2) + pow((uv.x - center.x), 2);
    float radiusQ = pow(radius, 2);
	
    if (circle < radiusQ)
    {
        output = smoothstep(radiusQ, radiusQ - smooth, circle);
    }
    else
        output = 0;
}