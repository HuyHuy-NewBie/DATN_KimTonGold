import 'package:flutter/material.dart';

import 'formatters.dart';
import 'models.dart';

class EmptyState extends StatelessWidget {
  const EmptyState({
    super.key,
    required this.icon,
    required this.title,
    this.message,
  });

  final IconData icon;
  final String title;
  final String? message;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 42, color: colors.outline),
            const SizedBox(height: 12),
            Text(
              title,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium,
            ),
            if (message != null) ...[
              const SizedBox(height: 6),
              Text(
                message!,
                textAlign: TextAlign.center,
                style: TextStyle(color: colors.onSurfaceVariant),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class LoadingPane extends StatelessWidget {
  const LoadingPane({super.key, this.label = 'Đang tải dữ liệu'});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const AnimatedLogoLoader(),
          const SizedBox(height: 12),
          Text(label),
        ],
      ),
    );
  }
}

class AnimatedLogoLoader extends StatefulWidget {
  const AnimatedLogoLoader({super.key, this.size = 64});

  final double size;

  @override
  State<AnimatedLogoLoader> createState() => _AnimatedLogoLoaderState();
}

class _AnimatedLogoLoaderState extends State<AnimatedLogoLoader>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1000),
    )..repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        return CustomPaint(
          size: Size(widget.size, widget.size),
          painter: _LogoRevealPainter(
            progress: _controller.value,
            color: colors.primary,
          ),
        );
      },
    );
  }
}

class _LogoRevealPainter extends CustomPainter {
  _LogoRevealPainter({required this.progress, required this.color});

  final double progress;
  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.fill;

    // Draw the diamond icon path
    final path = _getDiamondPath(size);

    // Create a circular reveal mask that moves from left to right
    // Circle center moves from x = -radius to x = width + radius
    final radius = size.width * 0.6;
    final centerX = -radius + (size.width + 2 * radius) * progress;
    final centerY = size.height / 2;

    canvas.saveLayer(Offset.zero & size, Paint());

    // Draw the diamond shape with partial opacity as background
    canvas.drawPath(path, Paint()..color = color.withValues(alpha: 0.1));

    // Clip with the moving circle
    final maskPath = Path()
      ..addOval(Rect.fromCircle(center: Offset(centerX, centerY), radius: radius));
    canvas.clipPath(maskPath);

    // Draw the solid diamond shape inside the mask
    canvas.drawPath(path, paint);

    canvas.restore();
  }

  Path _getDiamondPath(Size size) {
    final w = size.width;
    final h = size.height;
    return Path()
      ..moveTo(w * 0.5, h * 0.1) // Top center
      ..lineTo(w * 0.9, h * 0.35) // Top right
      ..lineTo(w * 0.5, h * 0.9) // Bottom center
      ..lineTo(w * 0.1, h * 0.35) // Top left
      ..close()
      // Add diamond facets lines (simplified as a path)
      ..moveTo(w * 0.1, h * 0.35)
      ..lineTo(w * 0.9, h * 0.35)
      ..moveTo(w * 0.3, h * 0.1)
      ..lineTo(w * 0.1, h * 0.35)
      ..moveTo(w * 0.7, h * 0.1)
      ..lineTo(w * 0.9, h * 0.35)
      ..moveTo(w * 0.5, h * 0.1)
      ..lineTo(w * 0.3, h * 0.35)
      ..moveTo(w * 0.5, h * 0.1)
      ..lineTo(w * 0.7, h * 0.35)
      ..moveTo(w * 0.3, h * 0.35)
      ..lineTo(w * 0.5, h * 0.9)
      ..moveTo(w * 0.7, h * 0.35)
      ..lineTo(w * 0.5, h * 0.9);
  }

  @override
  bool shouldRepaint(_LogoRevealPainter oldDelegate) =>
      oldDelegate.progress != progress;
}

class MarketDashboard extends StatelessWidget {
  const MarketDashboard({super.key, required this.data});

  final MarketSnapshot data;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            const Icon(Icons.show_chart, size: 20, color: Colors.orange),
            const SizedBox(width: 8),
            Text(
              'Tỷ giá vàng & bạc',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.w700,
              ),
            ),
            const Spacer(),
            if (data.retrievedAtUtc != null)
              Text(
                'Cập nhật: ${displayDateTime(data.retrievedAtUtc)}',
                style: Theme.of(context).textTheme.bodySmall,
              ),
          ],
        ),
        const SizedBox(height: 12),
        LayoutBuilder(
          builder: (context, constraints) {
            return GridView.count(
              crossAxisCount: 2,
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              mainAxisSpacing: 8,
              crossAxisSpacing: 8,
              childAspectRatio: 1.8,
              children: [
                _MetalCard(
                  title: 'Vàng Giao Ngay',
                  symbol: data.gold.displayName,
                  price: data.gold.price,
                  change: data.gold.change24h.percent,
                  color: Colors.amber.shade700,
                ),
                _MetalCard(
                  title: 'Biến Động Vàng',
                  symbol: '24H',
                  price: data.gold.change24h.amount,
                  change: data.gold.change24h.percent,
                  color: Colors.amber.shade800,
                  isChange: true,
                ),
                _MetalCard(
                  title: 'Bạc Giao Ngay',
                  symbol: data.silver.displayName,
                  price: data.silver.price,
                  change: data.silver.change24h.percent,
                  color: Colors.blueGrey,
                ),
                _MetalCard(
                  title: 'Biến Động Bạc',
                  symbol: '24H',
                  price: data.silver.change24h.amount,
                  change: data.silver.change24h.percent,
                  color: Colors.blueGrey.shade700,
                  isChange: true,
                ),
              ],
            );
          },
        ),
      ],
    );
  }
}

class _MetalCard extends StatelessWidget {
  const _MetalCard({
    required this.title,
    required this.symbol,
    required this.price,
    required this.change,
    required this.color,
    this.isChange = false,
  });

  final String title;
  final String symbol;
  final double price;
  final double change;
  final Color color;
  final bool isChange;

  @override
  Widget build(BuildContext context) {
    final isUp = change >= 0;
    return Container(
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withValues(alpha: 0.2)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Expanded(
                child: Text(
                  title,
                  style: TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w600,
                    color: color,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              Text(
                symbol,
                style: TextStyle(
                  fontSize: 10,
                  fontWeight: FontWeight.w700,
                  color: color.withValues(alpha: 0.6),
                ),
              ),
            ],
          ),
          Text(
            isChange
                ? '${isUp ? '+' : ''}${money(price)}'
                : money(price),
            style: TextStyle(
              fontSize: 15,
              fontWeight: FontWeight.w800,
              color: (color as MaterialColor).shade900,
            ),
          ),
          Row(
            children: [
              Icon(
                isUp ? Icons.arrow_drop_up : Icons.arrow_drop_down,
                size: 16,
                color: isUp ? Colors.green : Colors.red,
              ),
              Text(
                '${isUp ? '+' : ''}${change.toStringAsFixed(2)}%',
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.w700,
                  color: isUp ? Colors.green.shade700 : Colors.red.shade700,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}


class MetricTile extends StatelessWidget {
  const MetricTile({
    super.key,
    required this.icon,
    required this.label,
    required this.value,
    this.color,
  });

  final IconData icon;
  final String label;
  final String value;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    final tint = color ?? colors.primary;
    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(color: colors.outlineVariant),
        borderRadius: BorderRadius.circular(8),
        color: colors.surface,
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Row(
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: tint.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(icon, color: tint),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    label,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(color: colors.onSurfaceVariant),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    value,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class StatusChip extends StatelessWidget {
  const StatusChip(this.status, {super.key});

  final String status;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    final (color, labelColor) = switch (status) {
      'Hoàn thành' ||
      'Còn hàng' ||
      'Mới' => (Colors.green, Colors.green.shade700),
      'Chờ phê duyệt' ||
      'Đang xử lý' ||
      'Bán chạy' => (Colors.orange, Colors.orange.shade800),
      'Vận chuyển' => (Colors.blue, Colors.blue.shade700),
      'Đã hủy' || 'Đã xóa' || 'Hết hàng' => (Colors.red, Colors.red.shade700),
      _ => (colors.outline, colors.onSurfaceVariant),
    };
    return Chip(
      label: Text(status, maxLines: 1, overflow: TextOverflow.ellipsis),
      visualDensity: VisualDensity.compact,
      side: BorderSide(color: color.withValues(alpha: 0.25)),
      backgroundColor: color.withValues(alpha: 0.10),
      labelStyle: TextStyle(color: labelColor),
    );
  }
}

class BranchDropdown extends StatelessWidget {
  const BranchDropdown({
    super.key,
    required this.branches,
    required this.value,
    required this.onChanged,
    this.includeAll = true,
    this.label = 'Chi nhánh',
  });

  final List<Branch> branches;
  final int? value;
  final ValueChanged<int?> onChanged;
  final bool includeAll;
  final String label;

  @override
  Widget build(BuildContext context) {
    final items = <DropdownMenuItem<int?>>[
      if (includeAll)
        const DropdownMenuItem<int?>(
          value: null,
          child: Text('Tất cả chi nhánh'),
        ),
      ...branches.map(
        (branch) => DropdownMenuItem<int?>(
          value: branch.id,
          child: Text(branch.branchName, overflow: TextOverflow.ellipsis),
        ),
      ),
    ];
    return DropdownButtonFormField<int?>(
      initialValue: value,
      items: items,
      onChanged: onChanged,
      isExpanded: true,
      decoration: InputDecoration(labelText: label),
    );
  }
}

class SectionTitle extends StatelessWidget {
  const SectionTitle({super.key, required this.title, this.action});

  final String title;
  final Widget? action;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(4, 18, 4, 10),
      child: Row(
        children: [
          Expanded(
            child: Text(title, style: Theme.of(context).textTheme.titleMedium),
          ),
          ?action,
        ],
      ),
    );
  }
}

class RevenueBar extends StatelessWidget {
  const RevenueBar({
    super.key,
    required this.label,
    required this.value,
    required this.maxValue,
    this.trailing,
  });

  final String label;
  final double value;
  final double maxValue;
  final String? trailing;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    final percent = maxValue <= 0 ? 0.0 : (value / maxValue).clamp(0.04, 1.0);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  label,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              Text(
                trailing ?? money(value),
                style: Theme.of(context).textTheme.labelLarge,
              ),
            ],
          ),
          const SizedBox(height: 6),
          ClipRRect(
            borderRadius: BorderRadius.circular(4),
            child: LinearProgressIndicator(
              minHeight: 8,
              value: percent,
              backgroundColor: colors.surfaceContainerHighest,
            ),
          ),
        ],
      ),
    );
  }
}

Future<bool> confirmAction(
  BuildContext context, {
  required String title,
  required String message,
  String confirmLabel = 'Đồng ý',
}) async {
  final result = await showDialog<bool>(
    context: context,
    builder: (context) => AlertDialog(
      title: Text(title),
      content: Text(message),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context, false),
          child: const Text('Hủy'),
        ),
        FilledButton(
          onPressed: () => Navigator.pop(context, true),
          child: Text(confirmLabel),
        ),
      ],
    ),
  );
  return result == true;
}

void showSnack(BuildContext context, String message) {
  ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
}
