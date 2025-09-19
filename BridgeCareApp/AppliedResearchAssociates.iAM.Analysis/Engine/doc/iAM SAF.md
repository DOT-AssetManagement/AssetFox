# iAM Simulation Algorithm Framework <!-- omit in toc -->

The iAM _simulation algorithm framework_ (SAF) is an abstract, customizable
procedure using extensible data definitions that facilitates automated selection
of _projects_ to optimize the maintenance of _deteriorating assets_ over a
period of time.

An _SAF implementation_ (SAFI) is an artifact that uses the SAF procedure and
data, respectively customized and extended according to all requirements of the
SAFI.

- [1. Overview of Procedure](#1-overview-of-procedure)
- [2. Concepts \& Data Structures](#2-concepts--data-structures)
  - [2.1. Asset Snapshot](#21-asset-snapshot)
  - [2.2. Attribute Definition](#22-attribute-definition)
    - [2.2.1. Configuration](#221-configuration)
  - [2.3. Attribute Value Function](#23-attribute-value-function)
  - [2.4. Budget](#24-budget)
  - [2.5. Deficient Condition Goal](#25-deficient-condition-goal)
  - [2.6. Deterioration Profile](#26-deterioration-profile)
  - [2.7. Distribution-Cost Grade](#27-distribution-cost-grade)
  - [2.8. Function](#28-function)
  - [2.9. Funding Distribution Rule](#29-funding-distribution-rule)
  - [2.10. Future Prioritized Project Option Creation Rule](#210-future-prioritized-project-option-creation-rule)
  - [2.11. Future Treatment Exclusion Rule](#211-future-treatment-exclusion-rule)
  - [2.12. Predicate](#212-predicate)
  - [2.13. Project](#213-project)
  - [2.14. Project Option](#214-project-option)
  - [2.15. Project Option Creation Rule](#215-project-option-creation-rule)
  - [2.16. Scenario](#216-scenario)
    - [2.16.1. Configuration](#2161-configuration)
  - [2.17. Target Condition Goal](#217-target-condition-goal)
  - [2.18. Total Snapshot](#218-total-snapshot)
  - [2.19. Treatment](#219-treatment)
  - [2.20. Treatment Concurrency Exclusion Rule](#220-treatment-concurrency-exclusion-rule)

## 1. Overview of Procedure

An input dataset for the SAF is called a _scenario_. Acting on this dataset, the
basic procedure in the SAF is a sequence of _simulation time steps_. Each
simulation time step comprises four phases in sequence:

1. **Deterioration of assets**
   - The _deterioration profile_ of each _attribute definition_ is used across
     all assets in the simulation, excluding any currently _non-deteriorating
     attributes_ per asset.
2. **Selection of prioritized project options**
   - A pool of _prioritized project option creation rules_ is maintained for
     each asset during the simulation. Each prioritization is defined by a
     _treatment_ and is added to an asset's pool by a corresponding _project_
     when the latter is completed.
   - All _currently due_ prioritized project option creation rules are evaluated
     to generate a set of _prioritized project options_ from which selections
     are made in this phase.
   - If a prioritized project option is selected during this phase, the option's
     creation rule is removed from the asset's pool. If the option is not
     selected, the option's creation rule is deferred to the next time step's
     prioritized selection phase.
3. **Selection of other project options**
   - All _project option creation rules_ are evaluated against all assets to
     determine the set of _project options_ from which selections are made.
4. **Attribute value updates due to completed projects**
   - For each active project whose duration ends in this time step, the
     attribute value functions of that project's treatment are used to update
     the attribute values on that project's asset.

The SAF specifies the full behavior of phases 1 and 4 but only the general
behavior of phases 2 and 3. Within phases 2 and 3, an SAFI must specify two
major details of behavior:

1. **Calculation of _Estimated Objective Value (EOV)_**
   - EOV is a numeric value assigned to each project option based on
     characteristics of the project, e.g. attribute value updates due to the
     treatment; cost of the treatment; etc.
   - The "meaning" of EOV is entirely decided by an SAFI providing the
     calculation. The SAF does not prescribe the interpretation of EOV in any
     way.
   - To offer some examples, EOV might represent a desirable quantity to
     maximize, representing "benefit" or "remaining life" (optionally as a ratio
     against another metric like cost), or EOV might represent an undesirable
     quantity to minimize, perhaps representing long-term cost.
   - Calculation and interpretation of EOV is called an _EOV method_.
   - An SAFI may provide multiple EOV methods, and each EOV method may be
     configurable.
2. **Implementations of project option selection strategies based on EOV**
   - _Optimize selection subject to no constraints_
   - _Optimize selection subject to budget constraints_
   - _Optimize selection subject to goal constraints_
   - _Optimize selection subject to budget & goal constraints_
   - Budget constraints can always be met (by selecting no project options, thus
     spending no money).
   - Goal constraints can't always be met. When they can't be met, an SAFI has
     to decide what to do, i.e. continue somehow or abort.

After the final simulation time step, the primary output of an SAFI is the set
of projects selected during the simulation. The total set of projects for a
simulation period may include projects that were pre-selected as part of the
input dataset.

An SAFI may provide supplemental output data, such as a log describing internal
events and intermediate data.

## 2. Concepts & Data Structures

The subsections here are ordered for sequential reading. For reference, here is
an alphabetized list of links to each subsection:

### 2.1. Asset Snapshot

An asset snapshot is the full set of attribute values associated with a single
asset at a single moment in the sequence of steps and phases constituting the
simulation time sequence.

### 2.2. Attribute Definition

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

#### 2.2.1. Configuration

- For this attribute on a given asset, how are multiple simultaneous updates
  handled?
  - Any handling may be specified, as long as it is consistent and well-defined.
    Examples of handling might include:
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

### 2.3. Attribute Value Function

An attribute value function is an association between a function and an
attribute. The function's output is always a value for the associated attribute.

### 2.4. Budget

A budget describes a "pot of money" that can be used to fund projects.

- _Investment Sequence_: **one or more**
  - This is a sequence of money amounts, one per simulation time step. At each
    time step, the corresponding money amount is made available as part of this
    budget.
- _Predicate_: **optional**
  - If omitted, this budget is available for all assets.

### 2.5. Deficient Condition Goal

A deficient condition goal for an attribute is a statement that the SAF should
try to select projects such that the portion of applicable assets, each having a
value for this attribute that is "worse" (i.e. more deteriorated) than a
specified condition limit, is no larger than a specified deficiency limit.

- _Condition Limit_: **required**
- _Deficiency Limit_: **required**
  - The SAFI determines the interpretation of this limit. For example, it could
    be a percentage or a count.
- _Predicate_: **optional**
  - If omitted, this goal is applicable to all assets.

### 2.6. Deterioration Profile

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

### 2.7. Distribution-Cost Grade

- _Cost Ceiling_: **required**
  - If a treatment cost is less than or equal to this value, then the associated
    distribution sequence may be used to define funding and duration of a
    project using that treatment.
- _Distribution Sequence_: **one or more**
  - This is a sequence of percentage values, individually in the closed interval
    [0, 100], collectively summing to exactly 100.

### 2.8. Function

In the SAF, a function is a mapping from asset snapshots to numbers.

### 2.9. Funding Distribution Rule

A funding distribution rule specifies a set of "grades" that determine the cost
per time step and duration of a project option.

- _Predicate_: **optional**
  - If omitted, this rule is applicable to all assets.
- _Distribution-Cost Grades_: **one or more**

### 2.10. Future Prioritized Project Option Creation Rule

A future prioritized project option creation rule specifies a time after the
completion of a project at which point a specified project option creation rule
will be used to create a prioritized project option for the completed project's
associated asset.

- _Project Option Creation Rule_: **required**
- _Relative Time Offset_: **required**
  - This is a positive integer representing a number of simulation time steps.

### 2.11. Future Treatment Exclusion Rule

A future treatment exclusion rule specifies a length of time after the
completion of a project in which one or more specified treatments are not
allowed to be applied to the project's associated asset.

- _Excluded Treatments_: **one or more**
- _Exclusion Period Length_: **required**
  - This is a positive integer representing a number of simulation time steps.

### 2.12. Predicate

In the SAF, a predicate is a mapping from asset snapshots to classical truth
values, i.e. "true" or "false".

### 2.13. Project

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

### 2.14. Project Option

A project option is associated with one asset and describes a project available
for selection.

- _Creation Rule_: **required**
- _Cost Sequence_ : **required**
  - This is a sequence of costs, one per time step going forward, that must be
    paid in order to select this project option. The length of this sequence is
    the time step duration of the project this option represents.
- _Estimated Objective Value (EOV)_

### 2.15. Project Option Creation Rule

A project option creation rule specifies how to calculate total cost of a
treatment applied to an asset.

- _Predicate_: **optional**
  - If omitted, this rule is applicable to all assets.
- _Treatment_: **required**
- _Cost Function_: **required**
- _Allowed Budgets_: **one or more**
  - If (a) a budget is on this list and (b) that budget's predicate is satisfied
    by an asset, then the project option created by this rule for that asset is
    allowed to receive funding from that budget.

### 2.16. Scenario

A scenario is a container of all input data required for a single simulation
within an SAFI.

- _Attribute Definitions_: **one or more**
- _Initial Total Snapshot_: **required**
- _Committed Projects_: **zero or more**
- _Project Option Creation Rules_: **one or more**
- _Treatment Concurrency Exclusion Rules_: **zero or more**
- _Budgets_: **one or more**
- _Funding Distribution Rules_: **zero or more**

#### 2.16.1. Configuration

- Are multiple budgets allowed to fund a single project?
- Are budget remainders allowed to carry forward to the next simulation time
  step?

### 2.17. Target Condition Goal

A target condition goal for an attribute is a statement that the SAF should try
to select projects such that the average value of the attribute (across all
assets satisfying an optionally specified predicate) is not "worse" (i.e. more
deteriorated) than a specified condition limit.

- _Condition Limit_: **required**
- _Simulation Time Step_: **optional**
  - If omitted, this goal is applicable to all time steps.
- _Predicate_: **optional**
  - If omitted, this goal is applicable to all assets.

### 2.18. Total Snapshot

This is a set of asset snapshots, one for each asset in the scenario, for a
single moment in the sequence of simulation time steps and phases.

### 2.19. Treatment

A treatment specifies how the attribute values of an asset are updated when the
treatment is fully applied at the end of a project.

- _Attribute Value Functions_: **one or more**
- _Non-deteriorating Attributes_ : **zero or more**
  - When a project is funded over multiple time steps, some attributes of the
    treated asset may temporarily stop deteriorating for the duration of the
    project.
- _Future Treatment Exclusion Rules_: **zero or more**
- _Future Prioritized Project Option Creation Rules_: **zero or more**

### 2.20. Treatment Concurrency Exclusion Rule

A treatment concurrency exclusion rule specifies a set of mutually exclusive
treatments. In other words, no two projects with treatments from this set are
allowed to be active on the same asset at any one time. (Note that a treatment
is always self-exclusive, regardless of the presence or absence of exclusion
rules.)

- _Mutually Exclusive Treatments_: **two or more**
- _Predicate_: **optional**
  - If omitted, this rule is applicable to all assets.
