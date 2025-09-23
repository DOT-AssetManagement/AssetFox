# AssetFox SAFI <!-- omit in toc -->

AssetFox is an asset management web application that contains an implementation
of the iAM SAF.

## Overview of Method Implementations

### EOV Calculation

- benefit
  - attribute
  - limit
  - weighting
- remaining life
  - attribute
  - limit
- either benefit or RL, not both
- can do a ratio against estimated cost
- "outlook" & its configuration
  - number of years

### Project Option Selection

- "consideration" & its configuration
  - budget order
  - budget priority levels

## Unique SAFI Terms & Corresponding SAF Concepts

| SAFI Term                           | SAF Concept                                     |
| ----------------------------------- | ----------------------------------------------- |
| Active Treatment                    | treatment                                       |
| Attribute Default Value             | -                                               |
| Attribute Value History             | -                                               |
| Benefit                             | an interpretation of EOV                        |
| Budget Order                        | -                                               |
| Budget Priority Level               | -                                               |
| Calculated Field                    | -                                               |
| Cash Flow Distribution Rule         | cost distribution grade                         |
| Cash Flow Rule                      | cost distribution rule                          |
| Criterion                           | predicate                                       |
| Equation                            | function                                        |
| Inflation Rate                      | -                                               |
| Minimum Project Cost Limit          | -                                               |
| Network                             | assets                                          |
| Number Attribute                    | attribute definition                            |
| Optimization Strategy               | EOV method                                      |
| Passive Treatment                   | -                                               |
| Performance Curve                   | deterioration model function                    |
| Performance Curve Adjustment Factor | -                                               |
| Performance Curve Shift Mode        | -                                               |
| Remaining Life                      | an interpretation of EOV                        |
| Roll-Forward                        | -                                               |
| Selectable Treatment                | project option creation rule                    |
| Spending Strategy                   | project option selection method                 |
| Text Attribute                      | -                                               |
| Treatment Bundle                    | implicit concurrency of projects                |
| Treatment Consequence               | attribute value function                        |
| Treatment Consideration Loop        | project option selection                        |
| Treatment Option                    | project option                                  |
| Treatment Outlook                   | the calculation of EOV                          |
| Treatment Scheduling                | future prioritized project option creation rule |
| Treatment Shadow                    | future treatment exclusion rule                 |
| Treatment Supersede Rule            | treatment concurrency exclusion rule            |

## SAFI-Specific Terms & Definitions

- **Attribute Default Value**
  - This is an extension on an attribute definition.
  - This value is used to fill certain gaps in _attribute value history_ during
    _roll-forward_.
- **Attribute Value History**
  - This is an extension on a scenario, replacing the initial total snapshot.
  - Attribute value history is used to accomplish _roll-forward_, which is a
    preprocessing step that calculates the initial total snapshot.
  - Conceptually, attribute value history is a set of 4-tuples, each with the
    following shape:
    - `(time step, asset, attribute, value)`
  - Attribute value history can contain any asset's attribute values for the
    first simulation time step and for time steps before the first simulation
    time step.
  - Each value in the history is interpreted as being from a snapshot taken just
    after phase 1 (deterioration) of the corresponding time step.
    - This allows the effects of deterioration in the first simulation time step
      to be ignored.
- **Benefit**
- **Budget Order**
  - This is the order in which budgets are examined when the SAFI is attempting
    to construct the funding matrix for selection of a project option under
    consideration.
- **Budget Priority Level**
  - This is a description of what portions of all budgets are available at a
    specified level of priority during project option selection.
- **Calculated Field**
  - This is a "virtual" attribute definition, one whose values are always
    calculated from other attribute values in a given asset snapshot.
- **Inflation Rate**
  - This is used to adjust treatment costs upward as the simulation progresses
    through its time step sequence.
- **Minimum Project Cost Limit**
  - Evaluation of a project option creation rule is aborted if the rule's
    treatment's total cost is below this limit.
- **Passive Treatment**
  - Sometimes referred to as "the no-treatment".
  - This is intended to represent an asset's attribute value changes in a single
    time step when no project is selected for that asset.
  - This "treatment" is usually responsible for incrementing an "age" attribute,
    but a deterioration model can be used to accomplish the same thing just as
    easily.
- **Performance Curve Adjustment Factor**
  - On a single asset, the adjustment factor associated with a performance curve
    modifies the rate of deterioration _if that curve's function is defined by
    an age-dependent spline_.
  - A factor above one decreases the rate of deterioration and a factor below
    one increases the rate of deterioration. For example, if an asset has aged
    by 1.0 (which is the typical aging from one time step to the next), an
    adjustment factor of 2.0 will effectively change the age difference to 0.5.
  - An adjustment factor is associated with a performance curve by the
    application of treatments to an asset. Each asset maintains its own set of
    associations.
- **Performance Curve Shift Mode**
  - This is a mode associated with each performance curve, specified as part of
    the scenario and unchanging over the course of the simulation.
  - Shift mode is only applicable to a performance curve _if that curve's
    function is defined by an age-dependent spline_.
  - Conceptually, shift mode is intended to allow an attribute to deterioriate
    based on its current value rather than based on the asset's age, since it is
    possible for an asset to underperform or overperform relative to age-based
    expectations.
    - Note that deterioration based on current value is already possible (and
      more naturally formulated) by defining the curve's function with a
      mathematical expression (which has the entire asset snapshot in scope)
      rather than a piecewise expression (which has only the "age" attribute in
      scope).
  - When shift mode is enabled for a performance curve, the deterioration
    calculation is slightly modified. Normally, the deteriorating asset's "age"
    value is mapped directly to an attribute value on the spline, and then that
    value replaces the current value of that attribute. In this mode, the
    current attribute value on the asset is mapped to an "apparent age" value
    via the spline's inverse function. (It is an input requirement that all
    performance curve splines be invertible.) The "previous age" is then
    calculated (using the curve adjustment factor, if present) and used to
    determine a ratio of apparent age to previous age. The current age is
    multiplied by this ratio, giving a "shifted age" which is finally mapped to
    the new attribute value on the spline.
- **Roll-Forward**
  - This is a preprocessing step that translates the SAFI's _attribute value
    history_ into the SAF's required _initial total snapshot_.
  - Roll-forward is basically a sequence of partial simulation time steps that
    happen before the first simulation time step. During each of these time
    steps, only phases 1 (deterioriation) and 4 (project completion) occur. In
    other words, there is no project option selection during roll-forward.
    - Committed projects are allowed during roll-forward.
  - The first roll-forward time step is the earliest time step in the attribute
    value history. Attribute values on each asset are pulled from that time step
    in history.
    - If there is no value for a certain attribute on a certain asset in the
      first time step, then the value "falls forward" to the earliest historical
      time step with a value for that attribute on that asset. If there is no
      such time step, the attribute's default value is used.
  - The first roll-forward time step skips phase 1 because historical attribute
    values are interpreted as being from snapshots just after phase 1 of each
    time step.
  - On each subsequent roll-forward time step, just after phase 1, each
    attribute value is replaced if and only if the history contains a value for
    that attribute on the corresponding asset in the current time step.
- **Text Attribute**
  - This is another kind of attribute definition provided by this SAFI.
  - A text attribute has none of the properties of SAF numeric attributes.
  - A text attribute can have a default value.

## Expressing Criteria & Equations

This SAFI primarily uses a mathematical expression language called
"Calculate-Evaluate" to allow flexible definition of its criteria and equations.

A small "change expression" language is available for simpler definition of
attribute value functions on treatments.

A basic "piecewise expression" language is also available for defining
functions, usually deterioration functions. Such an expression contains a series
of 2D points defining a linear spline. Within an asset snapshot, the
corresponding function has exactly one dependency attribute, "age", an attribute
definition for which the scenario is required to provide.

## Pending Clarifications

- **Calculated Field Timing**
  - Why have this? What is its purpose? "Post-deterioration timing" is useless
    and "pre-deterioration timing" is conceptually unmoored.
- **Passive Treatment**
  - Why have this? Deterioration suffices.
  - This is perhaps a legacy of older versions of the RoadCare SAFI having only
    piecewise expressions for defining deterioration functions as age-dependent
    splines. If such versions existed, then the natural advancement of age had
    no representation other than being a consequence of this "treatment".
  - **"Pre-application" after phase 1**
    - Allegedly for compatibility with the RoadCare SAFI. Regardless, the domain
      concept and justification for this behavior is unknown.

---

# possible content

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
- **Maximizing AFS**: This FS produces the maximum result among all results from
  the default function and any functions whose predicates are satisfied. If
  there are no results (no default function and no satisfied predicates), it
  could be considered an input error.
