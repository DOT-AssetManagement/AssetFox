# iAM Simulation Algorithm Framework

The iAM _simulation algorithm framework_ (SAF) is an abstract, customizable
procedure using extensible data definitions that facilitates automated selection
of _projects_ to optimize the maintenance of _deteriorating assets_ over a
period of time. An _SAF implementation_ (SAFI) is an artifact that uses the SAF
procedure and data, respectively customized and extended according to all
requirements of the SAFI.

The basic procedure in the SAF is a sequence of _simulation time steps_. Each
simulation time step comprises four phases:

1. Deterioration of assets.
2. Selection of prioritized projects.
3. Selection of other projects.
4. Application of effects from completed projects.

The SAF fully specifies behavioral profiles for phases 1 and 4. For phases 2 and
3, an SAFI must provide certain behaviors to fully specify behavioral profiles.

## Concepts & Data Structures

### Asset Snapshot

An asset snapshot is the full set of attribute values associated with a single
asset at a single moment in the sequence of steps and phases constituting the
simulation time sequence.

### Predicate

In the SAF, a predicate is a mapping from an asset snapshot to a classical truth
value, i.e. "true" or "false".

### Function

In the SAF, a function is a mapping from an asset snapshot to a number.

#### Function System

> Wait a sec. This is an implementation detail.

In the SAF, a function system is a function comprising several other functions
and predicates. A function system has one _default function_ and zero or more
_conditional functions_. A conditional function is paired with a predicate. The
default function is optional if and only if there is at least one conditional
function.

A function system (abbreviated FS here) can be defined to do anything with its
internal functions. Here are some examples:

- **Switching FS**: This FS produces the result of the one conditional function
  in the FS whose predicate is satisfied. If multiple predicates are satisfied,
  it could be considered an input error. If no predicates are satisfied, the
  default function is used. If there is no default function when it is required,
  it could be considered an input error.
- **Summing AFS**: This FS produces the result of summing the default function
  result (if present) with all results from functions whose predicates are
  satisfied. If there are no results (no default function and no satisfied
  predicates), it could be considered an input error.
- **Minimizing AFS**: This FS produces the minimum result among all results from
  the default function and any functions whose predicates are satisfied. If
  there are no results (no default function and no satisfied predicates), it
  could be considered an input error.
- **Maximizing AFS**: This AFS produces the maximum result among all results
  from the default function and any functions whose predicates are satisfied. If
  there are no results (no default function and no satisfied predicates), it
  could be considered an input error.

### Scenario

A scenario is a container of all input data required for a single simulation
within an SAFI.

#### Data

- _Attribute Definitions_: **one or more**
- _Initial Total Snapshot_: **required**
- _Committed Projects_: **zero or more**
- _Null Treatment_: **required**
- _Treatment Funding Rules_: **one or more**
- _Treatment Concurrency Exclusion Rules_: **zero or more**
- _Budgets_: **one or more**
- _Cash Flow Rules_: **zero or more**

#### Configuration

- Are multiple budgets allowed to fund a single project?
- Are budget remainders allowed to carry forward to the next simulation time
  step?

### Attribute Definition

An attribute definition describes a property associated with some or all assets
in the scope of the scenario. In the SAF, only numeric attributes are specified,
but SAFIs may freely define and incorporate other kinds of attribute, e.g. text
or date or virtual.

#### Data

- _Minimum Value_: **optional**
- _Maximum Value_: **optional**
- _Deterioration Profile_: **optional**

#### Configuration

- How are multiple simultaneous effects (on this attribute within a single
  asset) handled?
  - Any method may be specified, as long as it is consistent and well-defined.
    Examples include:
    - "Exclusive": Do not allow multiple effects. Mutually exclude the sources
      (project options and/or committed projects) of the effects. Since
      committed projects are obligatory, a project P1 whose completion coincides
      with that of a committed project P2 (on the same asset) will be excluded
      from selection if P1 and P2 have effects that overlap on one or more
      attributes. If P1 and P2 are both committed projects when there is such
      overlap, it is an input error.
    - "Additive": Apply a change equal to the sum of all effects' changes.
    - "Multiplicative": Apply a change ratio equal to the product of all
      effects' change ratios.
    - "Minimum": Take the most negative effect result.
    - "Maximum": Take the most positive effect result.

### Deterioration Profile

A deterioration profile describes how an attribute value changes over time as an
asset with that attribute deteriorates. The profile may include information
about "goals" for the attribute value, to guide project selection.

#### Data

- _Deterioration Function_: **required**
- _Direction of Deterioration_: **negative or positive**
- _Target Condition Goals_: **zero or more**
- _Deficient Condition Goals_: **zero or more**

### Target Condition Goal

### Deficient Condition Goal

### Total Snapshot

### Project

---

## overall remarks

- Certain aspects of AssetFox's implementation of the iAM simulation are not
  part of the iAM simulation framework. For example, "calculated field timing"
  is an AssetFox-specific item introduced by PennDOT into their implementation
  of the iAM simulation. Another example is "performance curve adjustment
  factors".

## plan of writing

- We want something that's both high-level and detailed. We want to know the
  purpose of things like "calculated field timing", but we don't necessarily
  care about describing how exactly that thing gets handled at the
  code/book-keeping level.
- Also, note we're distinguishing the overall framework algorithm from the
  yearly decision-making (project-picking) algorithm. Each has its own
  required/suppplemental inputs and outputs.

## sketch of content

Very general overview of basic inputs and logic.

Top-to-bottom "structured glossary". In glossary term discussion, focus on
**purpose** of the entity.

### inputs

- To the simulation, there is no network or "explorer". It's just a set of
  assets and a set of attributes.
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
  supersede/concurrency rules. it should not be handled by the framework.
- benefit/cost could reflect total of all "follow-up" (i.e. scheduled)
  treatments, but it's up to the SAFI.
- the quasi-required "age" attribute is not technically required. It's only used
  to facilitate handling of piecewise expressions as performance curves. In
  other words, if that kind of "special" expression could include the dependency
  attribute(s) in its specification, there's no need to have "special"
  attributes.
- alternative methods of defining functions may exist, e.g. "piecewise
  expressions" as used by RoadCare and AssetFox.

## draft of content


### Glossary of Terms & Structured Data

Italicized text is used to indicate parts of a data structure. When such text is
followed by a colon, the text after the colon clarifies the general type of data
in the named part. If the part is optional, a parenthesized question mark is
shown. If the part comprises other items, a parenthesized minimum count is
shown.

Also, some of the structures shown here have explicit "identifier" fields, which
is really only to make it clear that instances of those structures are referred
to in other structures. However, in a given implementation, all structures could
have identifiers, and it's probably beneficial to make it so.

- **Simulation Time Sequence**
  - This is the abstract sequence of time steps over which the simulation
    operates.
- **Asset**
  - This is any entity whose physical attributes (a) may deteriorate over time
    and (b) may be managed or maintained by applying treatments to the entity.
  - Within the simulation, an asset is nothing more than a unique identifier.
- **Asset Snapshot**
  - This is a set of values, at most one per attribute, describing a single
    asset at a single moment in the simulation time sequence.
  - A snapshot does not necessarily have a value for each attribute, as some
    attributes may be inapplicable to some assets.
- **Total Snapshot**
  - This is a set of asset snapshots, one for each asset in the scenario, for a
    single step in the simulation time sequence.
- **Asset Function (Numeric)**
  - This is a function that applies to an asset snapshot and evaluates to a real
    number.
- **Asset Predicate**
  - This is a logical predicate that applies to an asset snapshot and evaluates
    to a classical truth value (i.e. either "true" or "false").
- **Asset Function System (AFS)**
  - This is a default asset function alongside a set of asset predicate-function
    pairs.
  - The default function is optional if and only if there is at least one
    predicate-function pair.
  - An AFS **is** an asset function. It maps an asset snapshot to a real number
    based on the results of the predicates.
  - Abstractly, an AFS can **contain** other AFSs. Whether and how to do this is
    up to the SAFI.
  - A specific AFS can be made to do anything with its internal functions. Here
    are some examples of AFS types:
    - **Switching AFS**
      - This AFS produces the result of exactly one function in the AFS.
      - If there are multiple satisfied predicates, that can be considered an
        input error.
      - If there are no results (no default and no satisfied predicates), that
        can be considered an input error.
    - **Summing AFS**
      - This AFS produces the result of summing the default function result (if
        present) with all results from functions whose predicates are satisfied.
      - If there are no results (no default and no satisfied predicates), that
        can be considered an input error.
    - **Minimizing AFS**
      - This AFS produces the minimum result among all results from the default
        function and any functions whose predicates are satisfied.
      - If there are no results (no default and no satisfied predicates), that
        can be considered an input error.
    - **Maximizing AFS**
      - This AFS produces the maximum result among all results from the default
        function and any functions whose predicates are satisfied.
      - If there are no results (no default and no satisfied predicates), that
        can be considered an input error.
- **Target Condition Goal**
  - _Predicate_ : (?)
  - _Condition Limit_ : number
  - _Simulation Time Step_ : positive integer (?)
    - Optional; if not present, goal is applicable to all time steps.
- **Deficient Condition Goal**
  - _Predicate_ : (?)
  - _Condition Limit_ : number
  - _Deficiency Limit_ : percentage value in $[0,100)$
- **Attribute Value Effect**
  - _Target Attribute_
  - _Effect Model_ : asset function
- **Prioritized Future Treatment Funding Rule**
  - _Treatment Funding Rule_
  - _Relative Time Offset_ : positive integer
- **Future Treatment Exclusion Rule**
  - _Excluded Treatments_ : Treatment (1+)
  - _Exclusion Period Length_ : positive integer
- **Null Treatment**
  - _Attribute Value Effects_ : (0+)
    - No more than one per attribute.
- **Treatment**
  - _Identifier_
  - _Attribute Value Effects_ : (1+)
    - No more than one per attribute.
  - _Non-deteriorating Attributes_ : (0+)
    - applicable when part of a multi-step project
- **Treatment Funding Rule**
  - _Predicate_ : (?)
  - _Treatment_
  - _Cost Model_ : asset function
  - _Allowed Budgets_ : (1+)
  - _Future Treatment Exclusion Rules_ : (0+)
  - _Prioritized Future Treatment Funding Rules_ : (0+)
- **Treatment Concurrency Exclusion Rule**
  - _Predicate_ : (?)
  - _Mutually Exclusive Treatments_ : (2+)
- **Budget**
  - _Predicate_ : (?)
  - _Investment Sequence_ : (1+)
- **Cash Flow Rule**
  - _Predicate_ : (?)
  - _Distribution Rules_, each:
    - _Cost Ceiling_ : positive number
    - _Percentage Sequence_ : (1+), individually in $[0,100]$, must sum to 100
- **Project Option**
  - _Source Rule_ : Treatment Funding Rule
  - _Asset Identifier_
  - _Cost Sequence_ : (1+)
    - multiple via cash-flow
  - _Objective Value_
    - Calculation and interpretation are completely the responsibility of the
      SAFI.
    - example interpretations:
      - estimated total "value" for the typical selection mode of maximizing
        value (derived from "benefit" or "remaining life") while respecting
        budget constraints
      - estimated total cost for the less typical selection mode of minimizing
        cost while achieving goal constraints and respecting budget constraints
        - in this mode, what if there are no feasible solutions (that achieve
          goals)? do you fall back to maximizing value? do you abort? do you
          optimize on the goals? the latter sounds the most appealing, but can
          it be done? it's multi-objective optimization due to multiple goals.
- **Project**
  - _Simulation Time Step_ : positive integer
  - _Asset Identifier_
  - _Treatment_
  - _Funding Structure_
    - basically a matrix of numeric entries. columns are relative time steps,
      rows are budgets, and entries are amounts spent from a budget in a time
      step.

outstanding modeling problems & solutions:

- prioritization handling needs to be kept simple. all that prioritization means
  is that you get a "first round" of decisions that uses only the current crop
  of prioritized TFRs on the respective assets.
  - "deferrals" are something to consider, e.g. when a prioritized TFR can't
    apply "in time" due to applicability predicate failure or due to
    non-selection of its project option (i.e. budget limitations or lack of
    necessity re goals), it's simply pushed back to the next time step.
- treatment concurrency needs to be the default mode. specifically, we cannot
  think of a cash-flow period as a hands-off period (which it is, currently in
  AssetFox). things can still be worked on. the solution is to lean back on
  concurrency exclusion rules, with the added sub-rule that a treatment cannot
  be re-applied while the treatment is being performed, and the treatment is
  "present" for exclusion-checking during the cash-flow.

### Responsibilities of the SAFI

- (possibly) how to generate a project option from a TFR
  - really, I don't think we should "officially" open this up. if the objective
    value estimation needs extra stuff from this step, that's fine, the SAFI can
    allow the extra stuff to be tracked/calculated.
  - keeping this aspect closed and well-defined will make the documentation that
    much clearer and the framework that much more usable.
- how to estimate "objective value" of project options
  - basically the "outlook algorithm"
- how to select from a set of project options
  - basically the "consideration loop"

### Output Data

At the framework level (ignoring additional output requirements imposed by
SAFIs), the only output is a set of projects.

_Initial Total Snapshot_ + _Projects (Committed & Selected)_ = _Final Total Snapshot_

Decision-making rationale output should be part of logging, not part of the
primary output. (Currently, in AssetFox, rationale output is bundled with
project output, and the bundle's large size has been a technical obstacle on the
project.)

A design goal is that the user should be able to easily (with little or no
effort) feed some or all of the output project activity _back into_ the
simulation as "committed project" input. In other words, when the user is
crafting committed project input, they should be crafting nearly the same
structure as what appears in the output. Note that this flexibility needs to
also include cash-flow. Users need to be able to clearly and intuitively specify
cash-flow committed projects.
- This has been achieved, I think.

### Data Operations

_Simulation_

1. DETERIORATION
   1. For each asset,
      1. For each deteriorating attribute (excluding non-deteriorating
         attributes of any projects active on this asset),
         - Deteriorate the attribute.
2. PRIORITIZED PROJECT SELECTION
   - Using per-asset prioritized TFRs only, determine project options, estimate
     objective values, and determine selections.
3. PROJECT SELECTION
   - Using all TFRs over all assets, determine project options, estimate
     objective values, and determine selections.
4. PROJECT EFFECTS
   - Apply the effects of completed projects.
