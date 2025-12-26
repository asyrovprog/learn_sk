# Quiz Answers: Advanced Prompt Engineering Patterns

**Date:** December 10, 2025  
**Topic:** Advanced Prompt Engineering Patterns

---

## Answer Key

**Question 1:** **A** - Few-shot uses examples in the prompt, zero-shot does not
- **Reasoning:** Few-shot learning provides examples of the desired input-output pattern within the prompt itself, while zero-shot relies solely on the model's pre-trained knowledge and instruction following without examples.

**Question 2:** **B** - "Let's think step by step"
- **Reasoning:** This specific phrase (or similar variations) has been shown in research to trigger step-by-step reasoning in language models, which is the core of Chain-of-Thought prompting.

**Question 3:** **A, B, D** - Generates multiple reasoning paths, Reduces hallucinations through majority voting, Improves reliability of complex reasoning tasks
- **Reasoning:** Self-Consistency generates multiple independent reasoning paths and uses majority voting to select the most consistent answer. This reduces hallucinations and improves reliability. It does NOT eliminate the need for examples (C is incorrect).

**Question 4:** **B** - It explores multiple reasoning branches and backtracks if needed
- **Reasoning:** Tree-of-Thoughts explicitly maintains a tree of reasoning paths, allowing the model to explore different branches and backtrack from dead ends, unlike linear CoT which follows a single path.

**Question 5:** **B** - Reasoning and Acting
- **Reasoning:** ReAct stands for "Reasoning and Acting" - it's a pattern where the model alternates between reasoning about what to do next and taking actions (like calling tools or functions).

**Question 6:** **Y** (Yes)
- **Reasoning:** This is the defining characteristic of ReAct - it creates a loop of Thought → Action → Observation → Thought → Action, alternating between reasoning and acting.

**Question 7:** **A** - Using prompts to generate or improve other prompts
- **Reasoning:** Meta-prompting is the technique of using LLMs to generate, optimize, or improve prompts for other tasks. It's "prompting about prompting."

**Question 8:** **A, B, D** - Prompt chaining (sequential steps), Prompt ensembling (multiple models/prompts), Conditional branching in prompts
- **Reasoning:** All three are valid composition patterns. Prompt deletion (C) is not a recognized pattern - it's a distractor.

**Question 9:** **A** - Temperature = 0.0, with Self-Consistency (multiple samples)
- **Reasoning:** For reliable CoT reasoning, you want deterministic responses (Temperature = 0.0). Self-Consistency requires multiple samples to vote on the most consistent answer, improving reliability for complex reasoning tasks.

**Question 10:** **B** - It breaks complex tasks into manageable steps with intermediate validation
- **Reasoning:** The primary advantage of chaining is decomposition of complex tasks into simpler steps, with the ability to validate intermediate results. It's not necessarily faster (A), doesn't always use fewer tokens (C), and does require a kernel/orchestration system (D).

---

## Scoring

- **10 correct:** 100% - Excellent understanding!
- **9 correct:** 90% - Great job!
- **8 correct:** 80% - Very good!
- **7 correct:** 70% - Passing, but review weak areas
- **6 or below:** <70% - Please review materials and retry
