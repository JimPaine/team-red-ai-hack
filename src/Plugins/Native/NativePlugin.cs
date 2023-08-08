using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Orchestration;

namespace src.Skills.Native;

public class NativePlugin {
    [SKFunction, Description("Something")]
    [SKParameter("input", "string")]
    public string Something(string input) {
        return input;
    }
}