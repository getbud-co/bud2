namespace Bud.Domain.ValueObjects;

public readonly record struct IndicatorTargetDefinition
{
    private IndicatorTargetDefinition(
        QuantitativeIndicatorType? quantitativeType,
        decimal? minValue,
        decimal? maxValue,
        IndicatorUnit? unit,
        string? targetText)
    {
        QuantitativeType = quantitativeType;
        MinValue = minValue;
        MaxValue = maxValue;
        Unit = unit;
        TargetText = targetText;
    }

    public QuantitativeIndicatorType? QuantitativeType { get; }
    public decimal? MinValue { get; }
    public decimal? MaxValue { get; }
    public IndicatorUnit? Unit { get; }
    public string? TargetText { get; }

    public static IndicatorTargetDefinition Create(
        IndicatorType type,
        QuantitativeIndicatorType? quantitativeType,
        decimal? minValue,
        decimal? maxValue,
        IndicatorUnit? unit,
        string? targetText)
    {
        if (type == IndicatorType.Qualitative)
        {
            return new IndicatorTargetDefinition(
                null,
                null,
                null,
                null,
                string.IsNullOrWhiteSpace(targetText) ? null : targetText.Trim());
        }

        if (quantitativeType is null)
        {
            throw new DomainInvariantException("Tipo quantitativo é obrigatório para indicadores quantitativos.");
        }

        if (unit is null)
        {
            throw new DomainInvariantException("Unidade é obrigatória para indicadores quantitativos.");
        }

        return quantitativeType.Value switch
        {
            QuantitativeIndicatorType.KeepAbove => new IndicatorTargetDefinition(
                quantitativeType,
                RequireNonNegativeMinValue(minValue, quantitativeType.Value),
                null,
                unit,
                null),
            QuantitativeIndicatorType.KeepBelow => new IndicatorTargetDefinition(
                quantitativeType,
                null,
                RequireNonNegativeMaxValue(maxValue, quantitativeType.Value),
                unit,
                null),
            QuantitativeIndicatorType.KeepBetween => CreateKeepBetweenTarget(quantitativeType.Value, minValue, maxValue, unit),
            QuantitativeIndicatorType.Achieve => new IndicatorTargetDefinition(
                quantitativeType,
                null,
                RequireNonNegativeMaxValue(maxValue, quantitativeType.Value),
                unit,
                null),
            QuantitativeIndicatorType.Reduce => new IndicatorTargetDefinition(
                quantitativeType,
                null,
                RequireNonNegativeMaxValue(maxValue, quantitativeType.Value),
                unit,
                null),
            _ => throw new DomainInvariantException("Tipo quantitativo inválido.")
        };
    }

    private static IndicatorTargetDefinition CreateKeepBetweenTarget(
        QuantitativeIndicatorType quantitativeType,
        decimal? minValue,
        decimal? maxValue,
        IndicatorUnit? unit)
    {
        var normalizedMinValue = RequireNonNegativeMinValue(minValue, quantitativeType);
        var normalizedMaxValue = RequireNonNegativeMaxValue(maxValue, quantitativeType);
        _ = IndicatorRange.Create(normalizedMinValue, normalizedMaxValue);

        return new IndicatorTargetDefinition(
            quantitativeType,
            normalizedMinValue,
            normalizedMaxValue,
            unit,
            null);
    }

    private static decimal RequireNonNegativeMinValue(decimal? minValue, QuantitativeIndicatorType quantitativeType)
    {
        if (!minValue.HasValue)
        {
            throw new DomainInvariantException($"Valor mínimo é obrigatório para indicadores {quantitativeType}.");
        }

        if (minValue.Value < 0)
        {
            throw new DomainInvariantException("Valor mínimo deve ser maior ou igual a 0.");
        }

        return minValue.Value;
    }

    private static decimal RequireNonNegativeMaxValue(decimal? maxValue, QuantitativeIndicatorType quantitativeType)
    {
        if (!maxValue.HasValue)
        {
            throw new DomainInvariantException($"Valor máximo é obrigatório para indicadores {quantitativeType}.");
        }

        if (maxValue.Value < 0)
        {
            throw new DomainInvariantException("Valor máximo deve ser maior ou igual a 0.");
        }

        return maxValue.Value;
    }
}
