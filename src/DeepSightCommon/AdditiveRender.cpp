#include "pch.h"
#include "AdditiveRender.h"

namespace DeepSightCommon
{

    template<typename IntersectorT, typename SampleT>
    inline AdditiveRender<IntersectorT, SampleT>::
        AdditiveRender(const IntersectorT& inter, BaseCamera& camera)
        : mAccessor(inter.grid().getConstAccessor())
        , mCamera(&camera)
        , mPrimary(new IntersectorT(inter))
        , mShadow(new IntersectorT(inter))
        , mPrimaryStep(1.0)
        , mShadowStep(3.0)
        , mCutOff(0.005)
        , mLightGain(0.2)
        , mLightDir(Vec3R(0.3, 0.3, 0).unit())
        , mLightColor(0.7, 0.7, 0.7)
        , mAbsorption(0.1)
        , mScattering(1.5)
    {
        mClipMin = std::numeric_limits<ValueType>().min();
        mClipMax = std::numeric_limits<ValueType>().max();
    }

    template<typename IntersectorT, typename SampleT>
    inline AdditiveRender<IntersectorT, SampleT>::
        AdditiveRender(const AdditiveRender& other)
        : mAccessor(other.mAccessor)
        , mCamera(other.mCamera)
        , mPrimary(new IntersectorT(*(other.mPrimary)))
        , mShadow(new IntersectorT(*(other.mShadow)))
        , mPrimaryStep(other.mPrimaryStep)
        , mShadowStep(other.mShadowStep)
        , mCutOff(other.mCutOff)
        , mLightGain(other.mLightGain)
        , mLightDir(other.mLightDir)
        , mLightColor(other.mLightColor)
        , mAbsorption(other.mAbsorption)
        , mScattering(other.mScattering)
        , mClipMin(other.mClipMin)
        , mClipMax(other.mClipMax)
    {
    }
    template<typename IntersectorT, typename SampleT>
    inline void AdditiveRender<IntersectorT, SampleT>::
        render(bool threaded) const
    {
        tbb::blocked_range<size_t> range(0, mCamera->height());
        threaded ? tbb::parallel_for(range, *this) : (*this)(range);
    }

    template<typename IntersectorT, typename SampleT>
    void AdditiveRender<IntersectorT, SampleT>::set_clip_min(ValueType clipMin)
    {
        mClipMin = clipMin;
    }

    template<typename IntersectorT, typename SampleT>
    void AdditiveRender<IntersectorT, SampleT>::set_clip_max(ValueType clipMax)
    {
        mClipMax = clipMax;
    }

    template<typename IntersectorT, typename SampleT>
    inline void AdditiveRender<IntersectorT, SampleT>::
        operator()(const tbb::blocked_range<size_t>& range) const
    {
        SamplerType sampler(mAccessor, mShadow->grid().transform());//light-weight wrapper

        // Any variable prefixed with p (or s) means it's associated with a primary (or shadow) ray
        const Vec3R extinction = -mScattering - mAbsorption, One(1.0);
        const Vec3R albedo = mLightColor * mScattering / (mScattering + mAbsorption);//single scattering
        const Real sGain = mLightGain;//in-scattering along shadow ray
        const Real pStep = mPrimaryStep;//Integration step along primary ray in voxel units
        const Real sStep = mShadowStep;//Integration step along shadow ray in voxel units
        const Real cutoff = mCutOff;//Cutoff for density and transmittance

        RayType sRay(Vec3R(0), mLightDir);//Shadow ray
        for (size_t j = range.begin(), je = range.end(); j < je; ++j) {
            for (size_t i = 0, ie = mCamera->width(); i < ie; ++i) {
                Film::RGBA& bg = mCamera->pixel(i, j);
                bg.a = bg.r = bg.g = bg.b = 0;
                RayType pRay = mCamera->getRay(i, j);// Primary ray
                if (!mPrimary->setWorldRay(pRay)) continue;
                Vec3R pTrans(1.0), pLumi(0.0);

                Real pT0, pT1;
                while (mPrimary->march(pT0, pT1)) {
                    for (Real pT = pStep * ceil(pT0 / pStep); pT <= pT1; pT += pStep) {

                        Vec3R pPos = mPrimary->getWorldPos(pT);
                        const Real density = sampler.wsSample(pPos);
                        if (density < cutoff) continue;
                        if (density < mClipMin || density > mClipMax) continue;

                        pLumi += Vec3R(density);

                        continue;
                        const Vec3R dT = math::Exp(extinction * density * pStep);
                        Vec3R sTrans(1.0);
                        sRay.setEye(pPos);
                        if (!mShadow->setWorldRay(sRay)) continue;

                        Real sT0, sT1;
                        while (mShadow->march(sT0, sT1)) {
                            for (Real sT = sStep * ceil(sT0 / sStep); sT <= sT1; sT += sStep) {

                                const Real d = sampler.wsSample(mShadow->getWorldPos(sT));
                                if (d < cutoff) continue;
                                sTrans *= math::Exp(extinction * d * sStep / (1.0 + sT * sGain));
                                if (sTrans.lengthSqr() < cutoff) goto Luminance;//Terminate sRay
                            }//Integration over shadow segment
                        }// Shadow ray march
                    Luminance:
                        pLumi += albedo * sTrans * pTrans * (One - dT);
                        pTrans *= dT;
                        if (pTrans.lengthSqr() < cutoff) goto Pixel;  // Terminate Ray
                    }//Integration over primary segment
                }// Primary ray march

            Pixel:
                bg.r = static_cast<Film::RGBA::ValueT>(pLumi[0]);
                bg.g = static_cast<Film::RGBA::ValueT>(pLumi[1]);
                bg.b = static_cast<Film::RGBA::ValueT>(pLumi[2]);
                bg.a = static_cast<Film::RGBA::ValueT>(1.0f - pTrans.sum() / 3.0f);
            }//Horizontal pixel scan
        }//Vertical pixel scan
    }

    template class AdditiveRender<openvdb::tools::VolumeRayIntersector<FloatGrid>, openvdb::tools::BoxSampler>;

}