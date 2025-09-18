# iAM Simulation Algorithm Framework

The iAM _simulation algorithm framework_ (SAF) is an abstract, customizable
procedure using extensible data definitions that facilitates automated selection
of _projects_ to optimize the maintenance of _deteriorating assets_ over a
period of time. An _SAF implementation_ (SAFI) is an artifact that uses the SAF
procedure and data, respectively customized and extended according to all
requirements of the SAFI.

The basic procedure in the SAF is a sequence of _simulation time steps_. Each
simulation time step comprises four phases in sequence:

1. Deterioration of assets.
2. Selection of prioritized projects.
3. Selection of other projects.
4. Application of effects from completed projects.

The SAF fully specifies the behavior of phases 1 and 4. Within phases 2 and 3,
an SAFI must specify certain aspects of the behavior.

> This document describes the intent and basic data of the SAF. It does not
> describe or prescribe the customizations and extensions of any SAFI.

## Concepts & Data Structures

The subsections here are ordered for sequential reading. For later reference,
here is an alphabetized list of links to each subsection:

- [Asset Snapshot](#asset-snapshot)
- [Predicate](#predicate)
- [Function](#function)
- [Scenario](#scenario)
- [Attribute Definition](#attribute-definition)
- [Deterioration Profile](#deterioration-profile)
- [Target Condition Goal](#target-condition-goal)
- [Deficient Condition Goal](#deficient-condition-goal)
- [Total Snapshot](#total-snapshot)
- [Project](#project)


### Asset Snapshot

An asset snapshot is the full set of attribute values associated with a single
asset at a single moment in the sequence of steps and phases constituting the
simulation time sequence.

### Predicate

In the SAF, a predicate is a mapping from an asset snapshot to a classical truth
value, i.e. "true" or "false".

### Function

In the SAF, a function is a mapping from an asset snapshot to a number.

### Attribute Value Function

An attribute value function is an association between a function and an
attribute. The function's output is always a value for the associated attribute.

### Scenario

A scenario is a container of all input data required for a single simulation
within an SAFI.

- _Attribute Definitions_: **one or more**
- _Initial Total Snapshot_: **required**
- _Committed Projects_: **zero or more**
- _Project Option Creation Rules_: **one or more**
- _Treatment Concurrency Exclusion Rules_: **zero or more**
- _Budgets_: **one or more**
- _Funding Distribution Rules_: **zero or more**

#### Configuration

- Are multiple budgets allowed to fund a single project?
- Are budget remainders allowed to carry forward to the next simulation time
  step?

### Attribute Definition

An attribute definition describes a property associated with some or all assets
in the scope of the scenario. In the SAF, only numeric attributes are specified,
but SAFIs may freely define and incorporate other kinds of attribute, e.g. text
or date or virtual.

- _Minimum Value_: **optional**
  - If omitted, this attribute has no minimum value.
- _Maximum Value_: **optional**
  - If omitted, this attribute has no maximum value.
- _Deterioration Profile_: **optional**
  - If omitted, this attribute never deteriorates.

#### Configuration

- For this attribute on a given asset, how are multiple simultaneous updates
  handled?
  - Any method may be specified, as long as it is consistent and well-defined.
    Example methods might include:
    - "Exclusive": Do not allow multiple simultaneous updates. Mutually exclude
      the sources (project options and/or committed projects) of the updates.
      Since committed projects are pre-selected, a project P1 whose completion
      (or duration) coincides with that of a committed project P2 (on the same
      asset) will be excluded from selection if P1 and P2 have updates that
      overlap on one or more attributes. If P1 and P2 are both committed
      projects when there is such overlap, it is an input error.
    - "Additive": Apply a change equal to the sum of all updates' changes.
    - "Multiplicative": Apply a change ratio equal to the product of all
      updates' change ratios.
    - "Minimum": Take the most negative effect result.
    - "Maximum": Take the most positive effect result.

### Deterioration Profile

A deterioration profile describes how an attribute value changes over time as an
asset with that attribute undergoes deterioration. The profile may include
information about "goals" for the attribute value, to guide project selection.

- _Attribute Value Function_: **required**
  - This function's associated attribute is the one whose definition contains
    this deterioration profile.
- _Direction of Deterioration_: **negative or positive**
  - This is required for interpretation of goal condition limits.
- _Target Condition Goals_: **zero or more**
- _Deficient Condition Goals_: **zero or more**

### Target Condition Goal

A target condition goal for an attribute is a statement that the SAF should try
to select projects such that the average value of the attribute (across all
assets satisfying an optionally specified predicate) is not "worse" (i.e. more
deteriorated) than a specified condition limit.

- _Condition Limit_: **required**
- _Simulation Time Step_: **optional**
  - If omitted, this goal is applicable to all time steps.
- _Predicate_: **optional**
  - If omitted, this goal is applicable to all assets.

### Deficient Condition Goal

A deficient condition goal for an attribute is a statement that the SAF should
try to select projects such that the portion of applicable assets having a value
for this attribute that is "worse" (i.e. more deteriorated) than a specified
condition limit is no larger than a specified deficiency limit.

- _Condition Limit_: **required**
- _Deficiency Limit_: **required**
  - The SAFI determines the interpretation of this limit. For example, it could
    be a percentage or a count.
- _Predicate_: **optional**
  - If omitted, this goal is applicable to all assets.

### Total Snapshot

This is a set of asset snapshots, one for each asset in the scenario, for a
single moment in the sequence of simulation time steps and phases.

### Project

A project specifies funding for a treatment applied to an asset at a certain
time for a certain length of time.

- _Simulation Time Step_: **required**
- _Asset Identifier_: **required**
- _Treatment_: **required**
- _Funding Matrix_: **required**
  - In this matrix, one dimension corresponds to the sequence of time steps over
    which this project is active, and the other dimension corresponds to budgets
    from which project funding is drawn. Each entry is the amount spent from one
    budget in one time step to fund this project's treatment.

### Treatment

A treatment specifies how the attribute values of an asset are updated when the
treatment is fully applied at the end of a project.

- _Attribute Value Functions_: **one or more**
- _Non-deteriorating Attributes_ : **zero or more**
  - When a project is funded over multiple time steps, some attributes of the
    treated asset may temporarily stop deteriorating for the duration of the
    project.
- _Future Treatment Exclusion Rules_: **zero or more**
- _Future Prioritized Project Option Creation Rules_: **zero or more**

### Future Treatment Exclusion Rule

A future treatment exclusion rule specifies a length of time after the
completion of a project in which one or more specified treatments are not
allowed to be applied to the project's associated asset.

- _Excluded Treatments_: **one or more**
- _Exclusion Period Length_: **required**
  - This is a positive integer representing a number of simulation time steps.

### Future Prioritized Project Option Creation Rule

A future prioritized project option creation rule specifies a time after the
completion of a project at which point a specified project option creation rule
will be used to create a prioritized project option associated with 

- _Project Option Creation Rule_: **required**
- _Relative Time Offset_: **required**
  - This is a positive integer representing a number of simulation time steps.
  - Adding the offset to the current simulation time step gives the future
    simulation time step in which the associated project option creation rule
    will participate in the prioritized project selection phase.

### Project Option Creation Rule

- _Predicate_: **optional**
  - If omitted, this rule is applicable to all assets.
- _Treatment_: **required**
- _Cost Function_: **required**
- _Allowed Budgets_: **one or more**

### Treatment Concurrency Exclusion Rule

- _Predicate_: **optional**
  - If omitted, this rule is applicable to all assets.
- _Mutually Exclusive Treatments_ : (2+)

### Budget

- _Predicate_: **optional**
  - If omitted, this budget is available for all assets.
- _Investment Sequence_ : (1+)

### Funding Distribution Rule

- _Predicate_: **optional**
  - If omitted, this rule is applicable to all assets.
- _Distribution Rules_, each:
  - _Cost Ceiling_ : positive number
  - _Percentage Sequence_ : (1+), individually in $[0,100]$, must sum to 100

### Project Option

A project option is associated with one asset and describes a project available
for selection.

- _Creation Rule_: **required**
- _Cost Sequence_ : **required**
  - This is a sequence of costs, one per time step going forward, that must be
    paid in order to select this project option. The length of this sequence is
    the time step duration of the project this option represents.
- _Estimated Objective Value (EOV)_
  - Calculation and interpretation of the EOV are entirely the responsibility of
    an SAFI. For example, the EOV might be positive, measuring "benefit" or
    "remaining life" (optionally as a ratio against another metric like cost),
    or the EOV might be negative (directly measuring long-term cost). Or it can
    be positive regardless, and the optimization can switch between maximizing
    and minimizing.
  - (put this into detail section about the 4 phases) Despite the major
    responsibility of an SAFI to specify and use the EOV, the SAF can delineate
    some general selection strategies:
    - Optimize total EOV with no constraints
    - Optimize total EOV with budget constraints
    - Optimize total EOV with goal constraints
    - Optimize total EOV with budget & goal constraints
    - No selection
  - An SAFI must provide the logic for each of the SAF selection strategies.
  - Budget constraints can always be met (by selecting nothing and spending
    nothing). Goal constraints can't always be met. When they can't be met, an
    SAFI has to decide what to do, e.g. continue somehow or abort.

---

## sketch of content

Top-to-bottom "structured glossary". In glossary term discussion, focus on
**purpose** of the entity.

### Glossary of Terms & Structured Data

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
