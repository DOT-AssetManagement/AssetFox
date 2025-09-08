# iAM Simulation Algorithm Framework (SAF)



## overall remarks

- The simulation of iAM (formerly RoadCare) is an ARA algorithm framework (not
  code in itself, but still having an original reference implementation in the
  RoadCare desktop application). In contrast, "AssetFox" is a specific PennDOT
  web application that contains one implementation of that framework.
- Certain aspects of AssetFox's implementation of the iAM simulation are not
  part of the iAM simulation framework. For example, "calculated field timing"
  is an AssetFox-specific item introduced by PennDOT into their implementation
  of the iAM simulation. Another example is "performance curve adjustment
  factors".

## plan of writing

- We want something that's both high-level and detailed. We want to know why
  things like "calculated field timing" exist (answer to that one: no idea), but
  we don't necessarily care about describing how exactly that thing gets handled
  at the code/book-keeping level.
- Also, note we're distinguishing the overall framework algorithm from the
  yearly decision-making (project-picking) algorithm. Each has its own
  required/suppplemental inputs and outputs.

## sketch of content

### inputs

- To the simulation, there is no network or "explorer". It's just a set of
  assets and a set of attributes.
- assets
  - ID, name, description, whatever
  - list of attribute-value pairs
- attribute
  - either number (double) or text (string)
    - number type should probably be decimal, but it doesn't matter for the
      sake of this doc.
  - also, "calculated fields". Probably should be called "calculated
    attributes". But "calculation" is too general at the conceptual level, even
    though it has a specific meaning re CalculateEvaluate within the system.
  - min, max
    - no "default" within the simulation, as it's only used for attribute
      value initialization.
  - deterioration/performance curves
    - target attribute (number only; not calculated)
    - criterion for an asset to satisfy in order for this curve to apply to
      the target attribute
    - the curve itself is either
      - an interpolated function of special automatic "age" attribute, or
      - an "equation" expression that's a function of some subset of the asset's
        current attributes; this dependency subset can contain the curve's
        affected attribute, but it should not contain any other attribute which
        is (or depends on) an attribute subject to deterioration.
- treatment
  - consequences
  - cost
  - list of budgets that are allowed to fund this treatment
- budget
  - amount of new money per time step
  - list of criteria that an asset must satisfy in order to receive a
    treatment with funding from this budget
- cash-flow rules
- initial attribute values on each asset
  - This is fundamentally **input**, not necessarily a part of simulation logic.
    Currently, the simulation handles the calculation of initial attribute
    values based on historical attribute values, but it should be decoupled to a
    preprocessing step.
- algorithm-specific metadata, like budget priority levels, budget order, etc.
  - These things could be discarded if the algorithm changed. Other inputs
    cannot be discarded; they are essential to achieving the outputs.
- time step definition (traditionally, "year", but the time step is handled
  very abstractly)
- objective function
- condition goals
- spending strategy
- other settings:
  - yearly budget remainder allowed to roll over to next year?
  - multiple budgets allowed to fund a single treatment?
  - assets allowed to deteriorate during cash-flow?
  - assets grouped for treatment selection?

### flow per time step

- start with last step's "final values"
- deterioration applied where applicable
  - carries the attributes into the current phase of decision-making
  - previously .
  - an asset may not deteriorate under certain conditions, e.g. active
    cash-flow.
- determine project options, including null project options based on the null treatment
  - check treatment feasibility criteria, shadows, etc
- estimate the "objective value" of each project option
  - PLUG-IN: needs an input/output spec
- determine selected projects (i.e. decide on funding, e.g. budgets, cash-flow)
  - PLUG-IN: needs an input/output spec
- apply all projects to the attribute-state of assets
- done; next time step

### outputs

- a project activity schedule that shows what treatment decisions were made when
  and what the effects of those decisions were (on the attribute values of
  each asset)
  - each year, you're not really "generating a new schedule for that year",
    you're more "filling in more of the overall simulation period schedule
    based on that year's data".
- "reasons" why each decision was made
  - This would be completely obviated by using CP for the decision phase of
    each year.
  - This is an example of accidental output, specific to the "picker"
    algorithm.
- "what output" vs "why output"
  - core output is what-output, picker-specific output is why-output

### misc remarks

- treatment bundling should be implicitly performed by a CP abiding by
  supersede/exclusion rules. it should not be handled by the framework.
- benefit/cost should reflect total cost of all "follow-up" (i.e. scheduled)
  treatments
- need a list of AssetFox additions:
  - calculated field timing
  - treatment bundles
  - asset grouping
- the quasi-required "age" attribute is not technically required. It's only used
  to facilitate handling of piecewise expressions as performance curves.

### some potential new terms

- null treatment: what we currently call some variant of "the no-treatment"
- project: the combination of a treatment and an asset (i.e. the application of
  a treatment to an asset) over a certain time period (one or more time steps).
  - project activity: one time step of a project
  - null project: a single-step project whose treatment is the null treatment.
  - project option
  - selected project
  - committed project: defined by the user and by projects' treatments'
    required future treatments
- 
## draft of content

### Overview

- _Scenario_ | _Simulation_ | _Project Plan_

### Terms & Concepts

- _Asset-Logical Expression (ALE)_
  - an expression that evaluates to either true or false when _asset-attribute
    state_ substitutes a value for each parameter referenced in the expression
- _Asset-Numeric Expression (ANE)_
  - an expression that evaluates to a number when an _asset-attribute snapshot_
    substitutes values for each parameter referenced in the expression

### Input Data

- _Scenario_
  - **Initial** _Asset-Attribute States_; each:
    - Asset ID
    - Attribute Values
  - _Numeric Attribute Definitions_; each:
    - Name
    - Minimum Value (optional)
    - Maximum Value (optional)
    - _Deterioration Model_ (optional)
      - Direction of Deterioration (increasing or decreasing)
      - Default Function (optional if 1+ conditional functions)
        - ANE
      - Conditional Functions; each:
        - ALE
        - ANE
      - The function-ANEs here are not allowed to reference (directly or
        indirectly) any other attribute having a deterioration model.
  - _Textual Attribute Definitions_; each:
    - Name
  - _Numeric Parameter Definitions_; each:
    - Name
    - Default Function (optional if 1+ conditional functions)
      - ANE
    - Conditional Functions; each:
      - ALE
      - ANE
  - numeric attribute deterioration functions
  - null treatment
  - selectable treatments
- _attribute deterioration function_
  - target attribute
  - application requirement
    - asset-logical expression
  - asset-numeric expression **with no direct or indirect references to other
    deteriorating attributes**
    - the expression may reference the target attribute and it may reference
      non-deteriorating attributes and parameters.
  - alternative methods of defining functions may exist, e.g. "piecewise
    expressions" as used by RoadCare and AssetFox.
- _treatment_
  - consequences
  - cost
  - selection requirement
    - asset-logical expression
  - future treatment requirements
    - future offset (greater than zero)
    - treatment

### Output Data

- _Project Plan_

### Data Operations

- _Simulation_