#pragma once
#pragma managed(push, off)
#include <openvdb/tools/RayTracer.h>
#include <openvdb/Types.h>
#include <openvdb/math/BBox.h>
#include <openvdb/math/Ray.h>
#include <openvdb/math/Math.h>
#include <openvdb/tools/RayIntersector.h>
#include <openvdb/tools/Interpolation.h>
#include <openvdb/openvdb.h>
#pragma managed(pop)

#include <deque>
#include <iostream>
#include <fstream>
#include <limits>
#include <memory>
#include <string>
#include <type_traits>
#include <vector>

using namespace openvdb;
using namespace openvdb::tools;

namespace DeepSightCommon
{
    template <typename IntersectorT, typename SamplerT = tools::BoxSampler>
    class AdditiveRender
    {
    public:

        using GridType = typename IntersectorT::GridType;
        using RayType = typename IntersectorT::RayType;
        using ValueType = typename GridType::ValueType;
        using AccessorType = typename GridType::ConstAccessor;
        using SamplerType = tools::GridSampler<AccessorType, SamplerT>;
        static_assert(std::is_floating_point<ValueType>::value,
            "AdditiveRender requires a floating-point-valued grid");
        /// @brief Public method required by tbb::parallel_for.
        /// @warning Never call it directly.
        /// 
        /// @brief Constructor taking an intersector and a base camera.
        AdditiveRender(const IntersectorT& inter, BaseCamera& camera);

        /// @brief Copy constructor which creates a thread-safe clone
        AdditiveRender(const AdditiveRender& other);

        void set_clip_min(ValueType clipMin);
        void set_clip_max(ValueType clipMax);

        /// @brief Perform the actual (potentially multithreaded) volume rendering.
        void render(bool threaded = true) const;/// 
        void operator()(const tbb::blocked_range<size_t>& range) const;

    private:

        AccessorType mAccessor;
        BaseCamera* mCamera;
        std::unique_ptr<IntersectorT> mPrimary, mShadow;
        ValueType mClipMin, mClipMax;
        Real  mPrimaryStep, mShadowStep, mCutOff, mLightGain;
        Vec3R mLightDir, mLightColor, mAbsorption, mScattering;
    }; // Additive render


}

