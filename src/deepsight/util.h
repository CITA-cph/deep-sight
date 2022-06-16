#pragma once

#include <Eigen/Core>

int inline GetIntersection(float fDst1, float fDst2, Eigen::Vector3f P1, Eigen::Vector3f P2, Eigen::Vector3f& Hit) {
    if ((fDst1 * fDst2) >= 0.0f) return 0;
    if (fDst1 == fDst2) return 0;
    Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));
    return 1;
}

int inline InBox(Eigen::Vector3f Hit, Eigen::Vector3f B1, Eigen::Vector3f B2, const int Axis) {
    if (Axis == 1 && Hit.z() > B1.z() && Hit.z() < B2.z() && Hit.y() > B1.y() && Hit.y() < B2.y()) return 1;
    if (Axis == 2 && Hit.z() > B1.z() && Hit.z() < B2.z() && Hit.x() > B1.x() && Hit.x() < B2.x()) return 1;
    if (Axis == 3 && Hit.x() > B1.x() && Hit.x() < B2.x() && Hit.y() > B1.y() && Hit.y() < B2.y()) return 1;
    return 0;
}

// returns true if line (L1, L2) intersects with the box (B1, B2)
// returns intersection point in Hit
int CheckLineBox(Eigen::Vector3f B1, Eigen::Vector3f B2, Eigen::Vector3f L1, Eigen::Vector3f L2, Eigen::Vector3f& Hit)
{
    if (L2.x() < B1.x() && L1.x() < B1.x()) return false;
    if (L2.x() > B2.x() && L1.x() > B2.x()) return false;
    if (L2.y() < B1.y() && L1.y() < B1.y()) return false;
    if (L2.y() > B2.y() && L1.y() > B2.y()) return false;
    if (L2.z() < B1.z() && L1.z() < B1.z()) return false;
    if (L2.z() > B2.z() && L1.z() > B2.z()) return false;
    if (L1.x() > B1.x() && L1.x() < B2.x() &&
        L1.y() > B1.y() && L1.y() < B2.y() &&
        L1.z() > B1.z() && L1.z() < B2.z())
    {
        Hit = L1;
        return true;
    }
    if ((GetIntersection(L1.x() - B1.x(), L2.x() - B1.x(), L1, L2, Hit) && InBox(Hit, B1, B2, 1))
        || (GetIntersection(L1.y() - B1.y(), L2.y() - B1.y(), L1, L2, Hit) && InBox(Hit, B1, B2, 2))
        || (GetIntersection(L1.z() - B1.z(), L2.z() - B1.z(), L1, L2, Hit) && InBox(Hit, B1, B2, 3))
        || (GetIntersection(L1.x() - B2.x(), L2.x() - B2.x(), L1, L2, Hit) && InBox(Hit, B1, B2, 1))
        || (GetIntersection(L1.y() - B2.y(), L2.y() - B2.y(), L1, L2, Hit) && InBox(Hit, B1, B2, 2))
        || (GetIntersection(L1.z() - B2.z(), L2.z() - B2.z(), L1, L2, Hit) && InBox(Hit, B1, B2, 3)))
        return true;

    return false;
}