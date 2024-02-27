uniform float ClipHeight;
uniform bool DoClip;

bool Clip(float testHeight)
{
    if (!DoClip)
        return false;
    
    return (testHeight < ClipHeight);
}
