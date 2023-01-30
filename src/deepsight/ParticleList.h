//
// Adapted from https://groups.google.com/g/openvdb-forum/c/FVlcRDZEsQo by paulge...@gmail.com
//
#pragma once

#include "openvdb/openvdb.h"

namespace DeepSight
{

    /// <summary>
    /// 
    /// </summary>
    struct Particle
    {
        openvdb::Vec3R p, v;
        openvdb::Real  r;
    };


    class ParticleList {

    public:
        openvdb::Real           mRadius;
        openvdb::Real           mVelocity;
        std::vector<Particle> mParticleList;

        typedef openvdb::Vec3R  PosType;

        ParticleList(openvdb::Real rScale = 1, openvdb::Real vScale = 1)
            : mRadius(rScale), mVelocity(vScale) {}

        void add(const openvdb::Vec3R& p, const openvdb::Real& r,
            const openvdb::Vec3R& v = openvdb::Vec3R(0, 0, 0))
        {
            Particle pa;
            pa.p = p;
            pa.r = r;
            pa.v = v;
            mParticleList.push_back(pa);
        }

        // Return the total number of particles in list.
        // Always required!
        size_t size() const { return mParticleList.size(); };
        // Get the world space position of the nth particle.
        // Required by ParticledToLevelSet::rasterizeSphere(*this,radius).
        void getPos(size_t n, openvdb::Vec3R& pos) const { pos = mParticleList[n].p; };
        // Get the world space position and radius of the nth particle.
        // Required by ParticledToLevelSet::rasterizeSphere(*this).
        void getPosRad(size_t n, openvdb::Vec3R& pos, openvdb::Real& rad) const {
            pos = mParticleList[n].p;
            rad = mRadius * mParticleList[n].r;
        };
        // Get the world space position, radius and velocity of the nth particle.
        // Required by ParticledToLevelSet::rasterizeSphere(*this,radius).
        void getPosRadVel(size_t n, openvdb::Vec3R& pos, openvdb::Real& rad, openvdb::Vec3R& vel) const {
            pos = mParticleList[n].p;
            rad = mRadius * mParticleList[n].r;
            vel = mVelocity * mParticleList[n].v;
        };
        // Get the attribute of the nth particle. AttributeType is user-defined!
        // Only required if attribute transfer is enabled in ParticlesToLevelSet.
        void getAtt(size_t n, openvdb::Index32& att) const { att = openvdb::Index32(n); };
    };

}